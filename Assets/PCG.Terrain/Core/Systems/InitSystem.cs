using System.Runtime.InteropServices;
using PCG.Terrain.Common.Grid;
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
using Unity.Transforms;

namespace PCG.Terrain.Core.Systems
{
    public sealed unsafe class InitSystem : BaseSystem
    {
        #region DataStructures

        private struct Index2D
        {
            private int2 _value;

            public static implicit operator Index2D(int2 value)
            {
                return new Index2D {_value = value};
            }

            public static implicit operator int2(Index2D index2D)
            {
                return index2D._value;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TempNoiseMetaInfo
        {
            public Entity Entity;
            public int Fraction;
        }

        #endregion

        private ComponentGroup _noiseGroup;
        private ComponentGroup _meshGroup;
        private ComponentGroup _noiseMetaInfoGroup;

        #region Jobs

        [BurstCompile]
        private struct InitNoiseCalculation : IJobChunk
        {
            [WriteOnly] public ArchetypeChunkBufferType<NoiseCalculation> NoiseCalculationType;
            [WriteOnly] public NativeMultiHashMap<ArchetypeChunk, Index2D>.Concurrent NoiseMetaInfoRemap;
            public int Size;
            public int PartSize;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var noiseCalculations = chunk.GetBufferAccessor(NoiseCalculationType);
                for (var i = 0; i < chunk.Count; i++)
                {
                    noiseCalculations[i].ResizeUninitialized(PartSize);
                    NoiseMetaInfoRemap.Add(chunk, Location.Index2DFrom1D(firstEntityIndex + i, Size));
                }
            }
        }

        [BurstCompile]
        private struct InitArchetypeChunkNoiseMetaInfo : IJobChunk
        {
            public ArchetypeChunkBufferType<ArchetypeChunkNoiseMetaInfo> ArchetypeChunkNoiseMetaInfoType;

            public ArchetypeChunkComponentType<ArchetypeChunkCalculationIndicator>
                ArchetypeChunkCalculationIndicatorType;

            [ReadOnly] public ArchetypeChunkComponentType<ChunkHeader> ChunkHeaderType;
            [ReadOnly] public NativeMultiHashMap<ArchetypeChunk, Index2D> NoiseMetaInfoRemap;
            [ReadOnly] public EntityArray EntityArray;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var archetypeChunkNoiseMetaInfos = chunk.GetBufferAccessor(ArchetypeChunkNoiseMetaInfoType);
                var archetypeChunkCalculationIndicators =
                    (ArchetypeChunkCalculationIndicator*) chunk.GetNativeArray(ArchetypeChunkCalculationIndicatorType)
                        .GetUnsafePtr();
                var chunkHeaders = chunk.GetNativeArray(ChunkHeaderType);
                for (var i = 0; i < chunk.Count; i++)
                {
                    archetypeChunkCalculationIndicators[i] = CalculationIndicators.Default;

                    var archetypeChunkNoiseMetaInfo = archetypeChunkNoiseMetaInfos[i];
                    var chunkHeader = chunkHeaders[i];

                    var totalEntities = chunkHeader.ArchetypeChunk.Count;
                    var tempNoiseMetaInfos = stackalloc TempNoiseMetaInfo[totalEntities];
                    var index = totalEntities - 1;

                    if (!NoiseMetaInfoRemap.TryGetFirstValue(chunkHeader.ArchetypeChunk, out var index2D,
                        out var iterator))
                    {
                        continue;
                    }

                    do
                    {
                        InsertAt(tempNoiseMetaInfos, ref index, index2D);
                    } while (NoiseMetaInfoRemap.TryGetNextValue(out index2D, ref iterator));

                    FillUniqueNoiseMetaData(tempNoiseMetaInfos, totalEntities, archetypeChunkNoiseMetaInfo);
                }
            }

            private void InsertAt(in TempNoiseMetaInfo* tempNoiseMetaInfos, ref int index, in int2 value)
            {
                tempNoiseMetaInfos[index] = new TempNoiseMetaInfo
                {
                    Entity = EntityArray[value.y],
                    Fraction = value.x
                };
                index--;
            }

            private void FillUniqueNoiseMetaData(in TempNoiseMetaInfo* tempNoiseMetaInfos, in int size,
                in DynamicBuffer<ArchetypeChunkNoiseMetaInfo> archetypeChunkNoiseMetaInfos)
            {
                var archetypeChunkNoiseMetaInfo = new ArchetypeChunkNoiseMetaInfo
                {
                    Entity = tempNoiseMetaInfos[0].Entity,
                    StartingIndex = 0,
                    EndingIndex = 0,
                    StartingFraction = tempNoiseMetaInfos[0].Fraction,
                    CalculationIndicator = CalculationIndicators.Default
                };

                if (tempNoiseMetaInfos[size - 1].Entity != archetypeChunkNoiseMetaInfo.Entity)
                {
                    for (var i = 0; i < size; i++)
                    {
                        var tempNoiseMetaInfo = tempNoiseMetaInfos[i];
                        if (tempNoiseMetaInfo.Entity == archetypeChunkNoiseMetaInfo.Entity)
                        {
                            continue;
                        }

                        archetypeChunkNoiseMetaInfo.EndingIndex = i - 1;
                        archetypeChunkNoiseMetaInfos.Add(archetypeChunkNoiseMetaInfo);

                        archetypeChunkNoiseMetaInfo.StartingIndex = i;
                        archetypeChunkNoiseMetaInfo.Entity = tempNoiseMetaInfo.Entity;
                        archetypeChunkNoiseMetaInfo.StartingFraction = tempNoiseMetaInfo.Fraction;
                    }
                }

                archetypeChunkNoiseMetaInfo.EndingIndex = size - 1;
                archetypeChunkNoiseMetaInfos.Add(archetypeChunkNoiseMetaInfo);
            }
        }

        #endregion

        public InitSystem(ITerrainSettings terrainSettings) : base(terrainSettings)
        {
        }

        protected override void OnCreateManager()
        {
            base.OnCreateManager();
            _noiseGroup = GetComponentGroup(
                typeof(NoiseCalculation)
            );
            _meshGroup = GetComponentGroup(
                ComponentType.ReadOnly<Position>(),
                ComponentType.ReadOnly<RenderMesh>()
            );
            _noiseMetaInfoGroup = GetComponentGroup(
                typeof(ArchetypeChunkNoiseMetaInfo),
                typeof(ArchetypeChunkCalculationIndicator),
                ComponentType.ReadOnly<ChunkHeader>()
            );
        }

        protected override void OnUpdate()
        {
            var noiseMetaInfoRemap = new NativeMultiHashMap<ArchetypeChunk, Index2D>(
                TerrainSettings.ChunkCount * TerrainSettings.ArrayChunk,
                Allocator.TempJob
            );

            var jobHandle = new JobHandle();

            var initNoiseCalculationJob = new InitNoiseCalculation
            {
                NoiseCalculationType = GetArchetypeChunkBufferType<NoiseCalculation>(),
                NoiseMetaInfoRemap = noiseMetaInfoRemap.ToConcurrent(),
                Size = TerrainSettings.ArrayChunk,
                PartSize = Environment.ValuesPerEntity
            };
            jobHandle = initNoiseCalculationJob.Schedule(_noiseGroup, jobHandle);

            var initArchetypeChunkNoiseMetaInfoJob = new InitArchetypeChunkNoiseMetaInfo
            {
                ArchetypeChunkNoiseMetaInfoType = GetArchetypeChunkBufferType<ArchetypeChunkNoiseMetaInfo>(),
                ArchetypeChunkCalculationIndicatorType =
                    GetArchetypeChunkComponentType<ArchetypeChunkCalculationIndicator>(),
                ChunkHeaderType = GetArchetypeChunkComponentType<ChunkHeader>(true),
                NoiseMetaInfoRemap = noiseMetaInfoRemap,
                EntityArray = _meshGroup.GetEntityArray()
            };
            jobHandle = initArchetypeChunkNoiseMetaInfoJob.Schedule(_noiseMetaInfoGroup, jobHandle);

            jobHandle.Complete();

            noiseMetaInfoRemap.Dispose();
        }
    }
}