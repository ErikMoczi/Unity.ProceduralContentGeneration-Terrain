using PCG.Terrain.Common.Extensions;
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
using UnityEngine.Assertions;

namespace PCG.Terrain.Core.Systems
{
    [UpdateBefore(typeof(RenderMeshSystemV2))]
    [UpdateBefore(typeof(EndFrameTransformSystem))]
    public sealed class TerrainSystem : BaseSystem
    {
        private ComponentGroup _noiseGroup;
        private ComponentGroup _meshGroup;
        private ComponentGroup _noiseMetaInfoGroup;
        private ComponentGroup _followTargetGroup;
        private ComponentGroup _verticesGroup;

        private IdentifyChange _identifyChange;
        private CullingLoader _cullingLoader;
        private CalculateNoise _calculateNoise;
        private RenderAssignment _renderAssignment;

        private int _noiseOrderVersion = -1;
        private int _meshOrderVersion = -1;

        private NativeArray<ArchetypeChunk> _noiseArchetypeChunks;
        private NativeArray<ArchetypeChunk> _meshArchetypeChunks;
        private NativeArray<NoiseMetaInfo> _noiseMetaInfos;


        public TerrainSystem(ITerrainSettings terrainSettings) : base(terrainSettings)
        {
        }

        protected override void OnCreateManager()
        {
            base.OnCreateManager();
            _noiseGroup = GetComponentGroup(
                typeof(NoiseCalculation)
            );
            _meshGroup = GetComponentGroup(
                typeof(Position),
                ComponentType.ReadOnly<RenderMesh>()
            );
            _noiseMetaInfoGroup = GetComponentGroup(
                typeof(ArchetypeChunkNoiseMetaInfo),
                typeof(ArchetypeChunkCalculationIndicator),
                ComponentType.ReadOnly<ChunkHeader>()
            );
            _followTargetGroup = GetComponentGroup(IdentifyChange.EntityArchetypeQuery);
            _verticesGroup = GetComponentGroup(RenderAssignment.EntityArchetypeQuery);

            RequireForUpdate(_noiseGroup);

            _identifyChange = new IdentifyChange(
                EntityManager,
                _followTargetGroup,
                TerrainSettings.ChangeThreshold,
                math.int2(0f)
            );
            _cullingLoader = new CullingLoader();
            _calculateNoise = new CalculateNoise();
            _renderAssignment = new RenderAssignment(EntityManager, _verticesGroup);
        }

        protected override void OnDestroyManager()
        {
            base.OnDestroyManager();
            _cullingLoader.Dispose();
            _renderAssignment.Dispose();
            DisposeMeshArchetypeChunks();
            DisposeNoiseArchetypeChunks();
            DisposeNoiseMetaInfos();
        }

        protected override void OnUpdate()
        {
            UpdateMeshArchetypeChunks();
            UpdateNoiseArchetypeChunks();

            _identifyChange.HandleChangeFollowTargetPosition(_cullingLoader.HandleChangePosition);

            if (_cullingLoader.AvailableBlocksCount > 0)
            {
                RecalculateChanges();
            }
        }

        #region MeshArchetypeChunks

        private void UpdateMeshArchetypeChunks()
        {
            var positionOrderVersion = EntityManager.GetComponentOrderVersion<Position>();
            var meshInstanceRendererOrderVersion = EntityManager.GetComponentOrderVersion<RenderMesh>();
            var meshOrderVersion = math.min(positionOrderVersion, meshInstanceRendererOrderVersion);
            if (meshOrderVersion == _meshOrderVersion)
            {
                return;
            }

            DisposeMeshArchetypeChunks();
            _meshArchetypeChunks = _meshGroup.CreateArchetypeChunkArray(Allocator.Persistent);

            _cullingLoader.Init(TerrainSettings.ChunkCount, _identifyChange.CentroidPosition,
                _meshArchetypeChunks.Impostor(), GetArchetypeChunkEntityType());

            _meshOrderVersion = meshOrderVersion;
        }

        private void DisposeMeshArchetypeChunks()
        {
            if (_meshArchetypeChunks.IsCreated)
            {
                _meshArchetypeChunks.Dispose();
            }
        }

        #endregion

        #region NoiseArchetypeChunks

        private void UpdateNoiseArchetypeChunks()
        {
            var noiseCalculationOrderVersion = EntityManager.GetComponentOrderVersion<NoiseCalculation>();
            if (noiseCalculationOrderVersion == _noiseOrderVersion)
            {
                return;
            }

            DisposeNoiseArchetypeChunks();
            _noiseArchetypeChunks = _noiseGroup.CreateArchetypeChunkArray(Allocator.Persistent);

            InitNoiseMetaInfos();
            _renderAssignment.Init(TerrainSettings.TotalVertices, _noiseMetaInfos.Impostor(),
                GetArchetypeChunkBufferType<NoiseCalculation>(true),
                _meshArchetypeChunks.Impostor(), GetArchetypeChunkEntityType(),
                TerrainSettings.ArrayChunk * TerrainSettings.ChunkCount
            );

            _noiseOrderVersion = noiseCalculationOrderVersion;
        }

        private void DisposeNoiseArchetypeChunks()
        {
            if (_noiseArchetypeChunks.IsCreated)
            {
                _noiseArchetypeChunks.Dispose();
            }
        }

        #endregion

        #region NoiseMetaInfos

        private void InitNoiseMetaInfos()
        {
            DisposeNoiseMetaInfos();
            _noiseMetaInfos = new NativeArray<NoiseMetaInfo>(_noiseArchetypeChunks.Length, Allocator.Persistent);

            new FillNoiseMetaInfos
            {
                ArchetypeChunkNoiseMetaInfoType = GetArchetypeChunkBufferType<ArchetypeChunkNoiseMetaInfo>(),
                ArchetypeChunkCalculationIndicatorType =
                    GetArchetypeChunkComponentType<ArchetypeChunkCalculationIndicator>(true),
                ChunkHeaderType = GetArchetypeChunkComponentType<ChunkHeader>(true),
                NoiseMetaInfos = _noiseMetaInfos
            }.Schedule(_noiseMetaInfoGroup).Complete();
        }

        private void DisposeNoiseMetaInfos()
        {
            if (_noiseMetaInfos.IsCreated)
            {
                _noiseMetaInfos.Dispose();
            }
        }

        [BurstCompile]
        private unsafe struct FillNoiseMetaInfos : IJobChunk
        {
            [WriteOnly] public ArchetypeChunkBufferType<ArchetypeChunkNoiseMetaInfo> ArchetypeChunkNoiseMetaInfoType;

            [ReadOnly] public ArchetypeChunkComponentType<ArchetypeChunkCalculationIndicator>
                ArchetypeChunkCalculationIndicatorType;

            [ReadOnly] public ArchetypeChunkComponentType<ChunkHeader> ChunkHeaderType;
            [NativeDisableParallelForRestriction] public NativeArray<NoiseMetaInfo> NoiseMetaInfos;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var archetypeChunkNoiseMetaInfos = chunk.GetBufferAccessor(ArchetypeChunkNoiseMetaInfoType);
                var archetypeChunkCalculationIndicators =
                    (ArchetypeChunkCalculationIndicator*) chunk.GetNativeArray(ArchetypeChunkCalculationIndicatorType)
                        .GetUnsafeReadOnlyPtr();
                var chunkHeaders = (ChunkHeader*) chunk.GetNativeArray(ChunkHeaderType).GetUnsafeReadOnlyPtr();
                for (var i = 0; i < chunk.Count; i++)
                {
                    var dynamicBuffer = archetypeChunkNoiseMetaInfos[i];
                    NoiseMetaInfos[firstEntityIndex + i] = new NoiseMetaInfo(
                        archetypeChunkCalculationIndicators,
                        (ArchetypeChunkNoiseMetaInfo*) dynamicBuffer.GetUnsafePtr(),
                        chunkHeaders,
                        dynamicBuffer.Length,
                        i
                    );
                }
            }
        }

        #endregion

        #region RecalculateChanges

        private void RecalculateChanges()
        {
            var changedMap = _cullingLoader.GetChangedMap(
                TerrainSettings.ChunksPerFrame,
                Allocator.TempJob,
                out var jobHandle
            );

            var prepareMetaDataForReCalculationJob = new PrepareMetaDataForReCalculation
            {
                ChangedMap = changedMap,
                NoiseMetaInfos = _noiseMetaInfos
            };
            jobHandle = prepareMetaDataForReCalculationJob.Schedule(_noiseMetaInfos.Length, 1, jobHandle);

            var updateMeshPositionsJob = new UpdateMeshPositions
            {
                ArchetypeChunks = _meshArchetypeChunks,
                PositionType = GetArchetypeChunkComponentType<Position>(),
                EntityType = GetArchetypeChunkEntityType(),
                ChangedMap = changedMap
            };
            jobHandle = updateMeshPositionsJob.Schedule(_meshArchetypeChunks.Length, 1, jobHandle);

            _calculateNoise.Run(
                GetArchetypeChunkBufferType<NoiseCalculation>(),
                _noiseMetaInfos,
                new MeshAbout(TerrainSettings.Resolution, TerrainSettings.NoiseSettings),
                Environment.ValuesPerEntity,
                Environment.TotalBufferEntities,
                ref jobHandle
            );

            jobHandle.Complete();

            _renderAssignment.ChangeRendering(changedMap, _noiseArchetypeChunks.Length, TerrainSettings.Resolution);
            Assert.IsTrue(changedMap.IsCreated);

            changedMap.Dispose();
        }

        #region Jobs

        [BurstCompile]
        private struct PrepareMetaDataForReCalculation : IJobParallelFor
        {
            [ReadOnly] public NativeHashMap<Entity, Offset> ChangedMap;
            [ReadOnly] public NativeArray<NoiseMetaInfo> NoiseMetaInfos;

            public void Execute(int index)
            {
                var noiseMetaInfo = NoiseMetaInfos[index];
                var archetypeChunkUsed = false;
                for (var i = 0; i < noiseMetaInfo.ArchetypeChunkNoiseMetaInfoSize; i++)
                {
                    ref var archetypeChunkNoiseMetaInfo = ref noiseMetaInfo[i];
                    if (!ChangedMap.TryGetValue(archetypeChunkNoiseMetaInfo.Entity, out var positionOffset))
                    {
                        archetypeChunkNoiseMetaInfo.CalculationIndicator = CalculationIndicator.Free;
                        continue;
                    }

                    archetypeChunkUsed = true;
                    archetypeChunkNoiseMetaInfo.CalculationIndicator = CalculationIndicator.Busy;
                    archetypeChunkNoiseMetaInfo.PositionOffset = positionOffset;
                }

                noiseMetaInfo.ArchetypeChunkCalculationIndicator =
                    archetypeChunkUsed ? CalculationIndicator.Busy : CalculationIndicator.Free;
            }
        }

        [BurstCompile]
        private unsafe struct UpdateMeshPositions : IJobParallelFor
        {
            [ReadOnly] public NativeArray<ArchetypeChunk> ArchetypeChunks;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;
            public ArchetypeChunkComponentType<Position> PositionType;
            [ReadOnly] public NativeHashMap<Entity, Offset> ChangedMap;

            public void Execute(int index)
            {
                var chunk = ArchetypeChunks[index];
                var entities = chunk.GetNativeArray(EntityType);
                var positions = (Position*) chunk.GetNativeArray(PositionType).GetUnsafePtr();
                for (var i = 0; i < chunk.Count; i++)
                {
                    if (!ChangedMap.TryGetValue(entities[i], out var changeData))
                    {
                        continue;
                    }

                    positions[i].Value = math.float3(changeData.Value.x, 0f, changeData.Value.y);
                }
            }
        }

        #endregion

        #endregion
    }
}