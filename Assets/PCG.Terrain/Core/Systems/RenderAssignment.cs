using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using PCG.Terrain.Common.Collections;
using PCG.Terrain.Common.Collections.Unsafe;
using PCG.Terrain.Common.Extensions;
using PCG.Terrain.Common.Grid;
using PCG.Terrain.Common.Helpers;
using PCG.Terrain.Common.Memory;
using PCG.Terrain.Core.Components;
using PCG.Terrain.Core.DataTypes;
using PCG.Terrain.Settings;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Assertions;
using NativeArrayExtensions = PCG.Terrain.Common.Extensions.NativeArrayExtensions;

namespace PCG.Terrain.Core.Systems
{
    public sealed class RenderAssignment : IDisposable
    {
        #region DataStructures

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct BufferNoiseEntity
        {
            public void* Pointer;
            public int Fraction;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MinMax
        {
            public float Min;
            public float Max;
        }

        #endregion

        public static readonly EntityArchetypeQuery EntityArchetypeQuery = new EntityArchetypeQuery
        {
            All = new[] {ComponentType.ReadOnly<VerticesData>()}
        };

        private readonly EntityManager _entityManager;
        private readonly ComponentGroup _componentGroup;
        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<Vector3> _normals = new List<Vector3>();

        private int _verticesDataOrderVersion = -1;

        private NativeHashMap<Entity, UnsafeArrayList<BufferNoiseEntity>> _remapNoiseEntities;
        private NativeUnit<MinMax> _globalElevationGap;

        #region Jobs

        [BurstCompile]
        private unsafe struct FillRemapNoiseEntities : IJobParallelFor
        {
            [ReadOnly] public ArchetypeChunkEntityType EntityType;
            [ReadOnly] public NativeArray<ArchetypeChunk> ArchetypeChunks;
            [ReadOnly] public NativeMultiHashMap<Entity, BufferNoiseEntity> OverallRemapHelper;
            [WriteOnly] public NativeHashMap<Entity, UnsafeArrayList<BufferNoiseEntity>>.Concurrent RemapNoiseEntities;

            public void Execute(int index)
            {
                var entity = ArchetypeChunks[index].GetNativeArray(EntityType)[0];
                OverallRemapHelper.TryGetFirstValueAndThrowOnFail(entity, out var bufferNoiseEntity, out var iterator);

                var unsafeArrayList = new UnsafeArrayList<BufferNoiseEntity>(
                    Allocator.Persistent,
                    MemoryOptions.UninitializedMemory
                );

                do
                {
                    unsafeArrayList.Add(new BufferNoiseEntity
                    {
                        Pointer = bufferNoiseEntity.Pointer,
                        Fraction = bufferNoiseEntity.Fraction
                    });
                } while (OverallRemapHelper.TryGetNextValue(out bufferNoiseEntity, ref iterator));

                RemapNoiseEntities.TryAddAndThrowOnFail(entity, unsafeArrayList);
            }
        }

        [BurstCompile]
        private unsafe struct CollectOverallNoiseEntities : IJobParallelFor
        {
            [ReadOnly] public NativeArray<NoiseMetaInfo> NoiseMetaInfos;
            [ReadOnly] public ArchetypeChunkBufferType<NoiseCalculation> NoiseCalculationType;
            [WriteOnly] public NativeMultiHashMap<Entity, BufferNoiseEntity>.Concurrent OverallRemapHelper;

            public void Execute(int index)
            {
                var noiseMetaInfo = NoiseMetaInfos[index];
                var chunk = noiseMetaInfo.ArchetypeChunk;
                var noiseCalculations = chunk.GetBufferAccessor(NoiseCalculationType);
                for (var i = 0; i < noiseMetaInfo.ArchetypeChunkNoiseMetaInfoSize; i++)
                {
                    var archetypeChunkNoiseMetaInfo = noiseMetaInfo[i];
                    for (int j = archetypeChunkNoiseMetaInfo.StartingIndex, currentFraction = 0;
                        j <= archetypeChunkNoiseMetaInfo.EndingIndex;
                        j++, currentFraction++)
                    {
                        OverallRemapHelper.Add(archetypeChunkNoiseMetaInfo.Entity, new BufferNoiseEntity
                        {
                            Pointer = noiseCalculations[j].AsNativeArray().GetUnsafeReadOnlyPtr(),
                            Fraction = archetypeChunkNoiseMetaInfo.CurrentFraction(currentFraction)
                        });
                    }
                }
            }
        }

        #endregion

        public RenderAssignment([NotNull] EntityManager entityManager, [NotNull] ComponentGroup componentGroup)
        {
#if DEBUG
            // ReSharper disable once JoinNullCheckWithUsage
            if (entityManager == null) throw new ArgumentNullException(nameof(entityManager));
            // ReSharper disable once JoinNullCheckWithUsage
            if (componentGroup == null) throw new ArgumentNullException(nameof(componentGroup));
#endif
            _entityManager = entityManager;
            _componentGroup = componentGroup;
        }

        public void Dispose()
        {
            DisposeOnChange();
        }

        public void Init(int totalVertices, in NativeArray<NoiseMetaInfo> noiseMetaInfos,
            in ArchetypeChunkBufferType<NoiseCalculation> noiseCalculationType,
            in NativeArray<ArchetypeChunk> archetypeChunks,
            in ArchetypeChunkEntityType entityType, int overallNoiseEntities)
        {
            UpdateVertices(totalVertices);
            DisposeOnChange();

            _globalElevationGap = new NativeUnit<MinMax>(Allocator.Persistent);

            var overallRemapHelper = new NativeMultiHashMap<Entity, BufferNoiseEntity>(
                overallNoiseEntities,
                Allocator.TempJob
            );
            _remapNoiseEntities = new NativeHashMap<Entity, UnsafeArrayList<BufferNoiseEntity>>(
                archetypeChunks.Length,
                Allocator.Persistent
            );

            var collectOverallNoiseEntitiesJob = new CollectOverallNoiseEntities
            {
                NoiseMetaInfos = noiseMetaInfos,
                OverallRemapHelper = overallRemapHelper.ToConcurrent(),
                NoiseCalculationType = noiseCalculationType
            };
            var jobHandle = collectOverallNoiseEntitiesJob.Schedule(noiseMetaInfos.Length, 1);

            var fillRemapNoiseEntitiesJob = new FillRemapNoiseEntities
            {
                OverallRemapHelper = overallRemapHelper,
                RemapNoiseEntities = _remapNoiseEntities.ToConcurrent(),
                ArchetypeChunks = archetypeChunks,
                EntityType = entityType
            };
            jobHandle = fillRemapNoiseEntitiesJob.Schedule(archetypeChunks.Length, 1, jobHandle);

            jobHandle.Complete();

            overallRemapHelper.Dispose();
        }

        private void DisposeOnChange()
        {
            if (_remapNoiseEntities.IsCreated)
            {
                using (var enumerator = _remapNoiseEntities.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var dat = enumerator.Current;
                        dat.Value.Dispose();
                    }
                }

                _remapNoiseEntities.Dispose();
            }

            if (_globalElevationGap.IsCreated)
            {
                _globalElevationGap.Dispose();
            }
        }

        private void UpdateVertices(int totalVertices)
        {
            var verticesDataOrderVersion = _entityManager.GetComponentOrderVersion<VerticesData>();
            if (verticesDataOrderVersion == _verticesDataOrderVersion)
            {
                return;
            }

            using (var archetypeChunks = _componentGroup.CreateArchetypeChunkArray(Allocator.TempJob))
            {
                Assert.AreEqual(archetypeChunks.Length, 1);
                Assert.AreEqual(archetypeChunks[0].Count, 1);

                var entityType = _entityManager.GetArchetypeChunkEntityType();
                var entity = archetypeChunks[0].GetNativeArray(entityType)[0];
                VerticesDataHandling.Collect(
                    _vertices,
                    totalVertices,
                    _entityManager.GetBuffer<VerticesData>(entity)
                );

                _normals.Capacity = _vertices.Capacity;
                NoAllocHelpers.ResizeList(_normals, _vertices.Capacity);
            }

            _verticesDataOrderVersion = verticesDataOrderVersion;
        }

        #region ChangeRendering

        public unsafe void ChangeRendering(in NativeHashMap<Entity, Offset> changedMap, int noiseArchetypeChunksCount,
            int resolution)
        {
            var vertices = NativeArrayExtensions.Impostor<float3>(
                _vertices.GetUnsafePtr(out var gcHandleVertices),
                _vertices.Count
            );
            var normals = NativeArrayExtensions.Impostor<float3>(
                _normals.GetUnsafePtr(out var gcHandleNormals),
                _normals.Count
            );

            using (var changedMapEnumerator = changedMap.GetEnumerator())
            {
                while (changedMapEnumerator.MoveNext())
                {
                    var value = changedMapEnumerator.Current;
                    var renderMesh = _entityManager.GetSharedComponentData<RenderMesh>(value.Key);

                    _remapNoiseEntities.TryGetAndThrowOnFail(value.Key, out var bufferNoiseEntitiesUnsafe);
                    var bufferNoiseEntities = bufferNoiseEntitiesUnsafe.AsNativeArray();

                    var innerBatchLoop =
                        (int) math.ceil((float) bufferNoiseEntities.Length / noiseArchetypeChunksCount);
                    var currentElevationGaps = new NativeArray<MinMax>(bufferNoiseEntities.Length, Allocator.TempJob);

                    var collectVerticesJob = new CollectVertices
                    {
                        Vertices = vertices,
                        BufferNoiseEntities = bufferNoiseEntities,
                        PartSize = Environment.ValuesPerEntity,
                        Size = Environment.TotalBufferEntities,
                        CurrentElevationGaps = currentElevationGaps
                    };
                    var collectVerticesJobHandle = collectVerticesJob.Schedule(
                        bufferNoiseEntities.Length,
                        innerBatchLoop
                    );

                    var recalculateElevationGapJob = new RecalculateElevationGap
                    {
                        GlobalElevationGap = _globalElevationGap,
                        CurrentElevationGaps = currentElevationGaps
                    };
                    var recalculateElevationGapJobHandle = recalculateElevationGapJob.Schedule(
                        collectVerticesJobHandle
                    );

                    var calculateNormalsJob = new CalculateNormals
                    {
                        Vertices = vertices,
                        Normals = normals,
                        BufferNoiseEntities = bufferNoiseEntities,
                        Resolution = resolution,
                        PartSize = Environment.ValuesPerEntity,
                        Size = Environment.TotalBufferEntities
                    };
                    var calculateNormalsJobHandle = calculateNormalsJob.Schedule(
                        bufferNoiseEntities.Length,
                        innerBatchLoop,
                        collectVerticesJobHandle
                    );

                    JobHandle.CombineDependencies(
                        recalculateElevationGapJobHandle,
                        calculateNormalsJobHandle
                    ).Complete();
                    currentElevationGaps.Dispose();

                    renderMesh.mesh.SetVertices(_vertices);
                    renderMesh.mesh.SetNormals(_normals);
                    renderMesh.material.SetVector(
                        TerrainSettings.ElevationMinMax,
                        new Vector2(_globalElevationGap.Value.Min, _globalElevationGap.Value.Max)
                    );
                }
            }

            UnsafeUtility.ReleaseGCObject(gcHandleVertices);
            UnsafeUtility.ReleaseGCObject(gcHandleNormals);
        }

        #region Jobs

        [BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
        private unsafe struct CalculateNormals : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float3> Vertices;
            [WriteOnly] public NativeArray<float3> Normals;
            [ReadOnly] public NativeArray<BufferNoiseEntity> BufferNoiseEntities;
            public int Size;
            public int PartSize;
            public int Resolution;

            public void Execute(int index)
            {
                var bufferNoiseEntity = BufferNoiseEntities[index];
                var fromIndex = bufferNoiseEntity.Fraction * Size;
                for (var i = 0; i < PartSize; i++)
                {
                    var currentIndex = fromIndex + i * 4;
                    var position = Location.Position(Location.NextIndexes(currentIndex), Resolution);
                    UnsafeUtility.WriteArrayElementWithStride(
                        Normals.GetUnsafePtr(),
                        currentIndex,
                        UnsafeUtility.SizeOf<float3>(),
                        Derivative.GetDerivative(Vertices, position, Resolution)
                    );
                }
            }
        }

        [BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
        private struct RecalculateElevationGap : IJob
        {
            [ReadOnly] public NativeArray<MinMax> CurrentElevationGaps;
            public NativeUnit<MinMax> GlobalElevationGap;

            public void Execute()
            {
                for (var i = 0; i < CurrentElevationGaps.Length; i++)
                {
                    var elevationGap = GlobalElevationGap.Value;
                    elevationGap.Min = math.min(CurrentElevationGaps[i].Min, elevationGap.Min);
                    elevationGap.Max = math.max(CurrentElevationGaps[i].Max, elevationGap.Max);
                    GlobalElevationGap.Value = elevationGap;
                }
            }
        }

        [BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
        private unsafe struct CollectVertices : IJobParallelFor
        {
            [WriteOnly] public NativeArray<float3> Vertices;
            [ReadOnly] public NativeArray<BufferNoiseEntity> BufferNoiseEntities;
            [WriteOnly] public NativeArray<MinMax> CurrentElevationGaps;
            public int Size;
            public int PartSize;

            public void Execute(int index)
            {
                var bufferNoiseEntity = BufferNoiseEntities[index];
                var fromIndex = bufferNoiseEntity.Fraction * Size;
                NoiseDataHandling.FillVector3Y(Vertices.GetUnsafePtr(), bufferNoiseEntity.Pointer, fromIndex, Size);

                float4 min = math.float4(float.MaxValue), max = math.float4(float.MinValue);
                for (var i = 0; i < PartSize; i++)
                {
                    var noiseData = UnsafeUtility.ReadArrayElement<float4>(bufferNoiseEntity.Pointer, i);
                    min = math.min(min, noiseData);
                    max = math.max(max, noiseData);
                }

                CurrentElevationGaps[index] = new MinMax
                {
                    Min = math.cmin(min),
                    Max = math.cmax(max)
                };
            }
        }

        #endregion

        #endregion
    }
}