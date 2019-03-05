using System;
using System.Runtime.InteropServices;
using PCG.Terrain.Common.Collections;
using PCG.Terrain.Common.Extensions;
using PCG.Terrain.Common.Grid;
using PCG.Terrain.Core.DataTypes;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace PCG.Terrain.Core.Systems
{
    public sealed class CullingLoader : IDisposable
    {
        #region DataStructures

        [StructLayout(LayoutKind.Sequential)]
        private struct BlockData
        {
            public Entity Entity;
            public int2 Offset;
        }

        #endregion

        private NativeQueue<BlockData> _finishedBlocks;
        private NativeQueue<BlockData> _availableBlocks;
        private NativeHashMap<Offset, Entity> _actualPositions;
        private NativeArray<int2> _baseOffsetPositions;
        private NativeUnit<int2> _centroidPosition;
        private int _chunkCount;

        public int AvailableBlocksCount => _availableBlocks.Count;

        #region Jobs

        [BurstCompile]
        private struct InitBaseBlocks : IJobParallelFor
        {
            [WriteOnly] public NativeHashMap<Offset, Entity>.Concurrent ActualPositions;
            [WriteOnly] public NativeQueue<BlockData>.Concurrent AvailableBlocks;
            [WriteOnly] public NativeArray<int2> BaseOffsetPositions;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;
            [ReadOnly] public NativeArray<ArchetypeChunk> ArchetypeChunks;
            public int2 CentroidPosition;

            public void Execute(int index)
            {
                var entity = ArchetypeChunks[index].GetNativeArray(EntityType)[0];
                var spiralChunkPosition = Location.SpiralChunkPosition(index);
                BaseOffsetPositions[index] = spiralChunkPosition;
                var offset = spiralChunkPosition + CentroidPosition;
                AvailableBlocks.Enqueue(new BlockData
                {
                    Entity = entity,
                    Offset = offset
                });
                ActualPositions.TryAddAndThrowOnFail(offset, entity);
            }
        }

        [BurstCompile]
        private struct RecalculationDataBaseOnFollowTarget : IJob, IDisposable
        {
            [WriteOnly] private NativeQueue<BlockData> _correctBlocks;
            [WriteOnly] private NativeQueue<BlockData> _incorrectBlocks;
            private NativeHashMap<Offset, Entity> _finishedBlocksRemapped;

            [WriteOnly] public NativeQueue<BlockData> FinishedBlocks;
            [WriteOnly] public NativeQueue<BlockData> AvailableBlocks;
            public NativeHashMap<Offset, Entity> ActualPositions;
            [ReadOnly] public NativeArray<int2> BaseOffsetPositions;
            [WriteOnly] public NativeUnit<int2> CentroidPosition;
            public float2 FollowTargetPosition;

            public void Init()
            {
                _correctBlocks = new NativeQueue<BlockData>(Allocator.TempJob);
                _incorrectBlocks = new NativeQueue<BlockData>(Allocator.TempJob);
                _finishedBlocksRemapped = new NativeHashMap<Offset, Entity>(
                    BaseOffsetPositions.Length,
                    Allocator.TempJob
                );
            }

            public void Execute()
            {
                var offsetIndex = Location.SpiralChunkIndex((int2) math.round(FollowTargetPosition));
                var newOffset = Location.SpiralChunkPosition(offsetIndex);
                CentroidPosition.Value = newOffset;

                while (FinishedBlocks.TryDequeue(out var blockData))
                {
                    _finishedBlocksRemapped.TryAddAndThrowOnFail(blockData.Offset, blockData.Entity);
                }

                for (var i = 0; i < BaseOffsetPositions.Length; i++)
                {
                    var offset = BaseOffsetPositions[i] + newOffset;
                    if (ActualPositions.TryGetValue(offset, out var entity))
                    {
                        var blockData = new BlockData
                        {
                            Entity = entity,
                            Offset = offset
                        };
                        if (_finishedBlocksRemapped.TryGetValue(offset, out _))
                        {
                            _correctBlocks.Enqueue(blockData);
                            ActualPositions.Remove(offset);
                        }
                        else
                        {
                            _incorrectBlocks.Enqueue(blockData);
                        }
                    }
                    else
                    {
                        _incorrectBlocks.Enqueue(new BlockData {Entity = Entity.Null, Offset = offset});
                    }
                }

                var stubData = ActualPositions.GetEnumerator();
                {
                    while (stubData.MoveNext())
                    {
                        var blockData = _incorrectBlocks.Dequeue();
                        blockData.Entity = stubData.Current.Value;
                        _incorrectBlocks.Enqueue(blockData);
                    }
                }
                stubData.Dispose();

                FinishedBlocks.Clear();
                AvailableBlocks.Clear();
                ActualPositions.Clear();

                while (_correctBlocks.TryDequeue(out var blockData))
                {
                    ActualPositions.TryAddAndThrowOnFail(blockData.Offset, blockData.Entity);
                    FinishedBlocks.Enqueue(blockData);
                }

                while (_incorrectBlocks.TryDequeue(out var blockData))
                {
                    ActualPositions.TryAddAndThrowOnFail(blockData.Offset, blockData.Entity);
                    AvailableBlocks.Enqueue(blockData);
                }
            }

            public void Dispose()
            {
                _correctBlocks.Dispose();
                _incorrectBlocks.Dispose();
                _finishedBlocksRemapped.Dispose();
            }
        }

        [BurstCompile]
        private struct GetChangedBlocks : IJob
        {
            [WriteOnly] public NativeQueue<BlockData> FinishedBlocks;
            [WriteOnly] public NativeQueue<BlockData> AvailableBlocks;
            [WriteOnly] public NativeHashMap<Entity, Offset> ChangedMap;
            public int ChunksPerFrame;

            public void Execute()
            {
                for (var i = 0; i < ChunksPerFrame && AvailableBlocks.TryDequeue(out var blockData); i++)
                {
                    FinishedBlocks.Enqueue(blockData);
                    ChangedMap.TryAddAndThrowOnFail(blockData.Entity, blockData.Offset);
                }
            }
        }

        #endregion

        public CullingLoader()
        {
            _centroidPosition = new NativeUnit<int2>(Allocator.Persistent);
        }

        public void Init(int chunkCount, int2 centroidPosition, in NativeArray<ArchetypeChunk> archetypeChunks,
            in ArchetypeChunkEntityType entityType)
        {
            DisposeOnChange();

            _chunkCount = chunkCount;
            _finishedBlocks = new NativeQueue<BlockData>(Allocator.Persistent);
            _availableBlocks = new NativeQueue<BlockData>(Allocator.Persistent);
            _actualPositions = new NativeHashMap<Offset, Entity>(chunkCount, Allocator.Persistent);
            _baseOffsetPositions = new NativeArray<int2>(chunkCount, Allocator.Persistent);

            new InitBaseBlocks
            {
                ArchetypeChunks = archetypeChunks,
                EntityType = entityType,
                AvailableBlocks = _availableBlocks.ToConcurrent(),
                ActualPositions = _actualPositions.ToConcurrent(),
                BaseOffsetPositions = _baseOffsetPositions,
                CentroidPosition = centroidPosition
            }.Schedule(archetypeChunks.Length, 1).Complete();

            Assert.AreEqual(_availableBlocks.Count, chunkCount);
            Assert.AreEqual(_actualPositions.Length, chunkCount);
        }

        public int2 HandleChangePosition(float2 followTargetPosition)
        {
            using (var recalculationDataBaseOnFollowTargetJob = new RecalculationDataBaseOnFollowTarget
            {
                AvailableBlocks = _availableBlocks,
                FinishedBlocks = _finishedBlocks,
                ActualPositions = _actualPositions,
                BaseOffsetPositions = _baseOffsetPositions,
                CentroidPosition = _centroidPosition,
                FollowTargetPosition = followTargetPosition
            })
            {
                recalculationDataBaseOnFollowTargetJob.Init();
                recalculationDataBaseOnFollowTargetJob.Schedule().Complete();
            }

            Assert.AreEqual(_actualPositions.Length, _chunkCount);
            Assert.AreEqual(_finishedBlocks.Count + _availableBlocks.Count, _actualPositions.Length);
            _actualPositions.FailOnDuplicateData();

            return _centroidPosition.Value;
        }

        public NativeHashMap<Entity, Offset> GetChangedMap(int chunksPerFrame, Allocator allocator,
            out JobHandle jobHandle)
        {
            var changedMap = new NativeHashMap<Entity, Offset>(chunksPerFrame, allocator);
            var getChangedBlocksJob = new GetChangedBlocks
            {
                AvailableBlocks = _availableBlocks,
                FinishedBlocks = _finishedBlocks,
                ChangedMap = changedMap,
                ChunksPerFrame = chunksPerFrame
            };
            jobHandle = getChangedBlocksJob.Schedule();

            return changedMap;
        }

        public void Dispose()
        {
            DisposeOnChange();

            if (_centroidPosition.IsCreated)
            {
                _centroidPosition.Dispose();
            }
        }

        private void DisposeOnChange()
        {
            if (_finishedBlocks.IsCreated)
            {
                _finishedBlocks.Dispose();
            }

            if (_availableBlocks.IsCreated)
            {
                _availableBlocks.Dispose();
            }

            if (_actualPositions.IsCreated)
            {
                _actualPositions.Dispose();
            }

            if (_baseOffsetPositions.IsCreated)
            {
                _baseOffsetPositions.Dispose();
            }
        }
    }
}