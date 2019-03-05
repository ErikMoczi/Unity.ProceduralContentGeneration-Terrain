using PCG.Terrain.Common.Grid;
using PCG.Terrain.Core.Components;
using PCG.Terrain.Core.DataTypes;
using PCG.Terrain.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace PCG.Terrain.Core.Systems
{
    public sealed class CalculateNoise
    {
        #region Jobs

        [BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
        private unsafe struct NoiseHandling : IJobParallelFor
        {
            public ArchetypeChunkBufferType<NoiseCalculation> NoiseCalculationType;
            [ReadOnly] public NativeArray<NoiseMetaInfo> NoiseMetaInfos;
            public MeshAbout MeshAbout;
            public int Size;
            public int PartSize;

            public void Execute(int index)
            {
                var noiseMetaInfo = NoiseMetaInfos[index];
                if (noiseMetaInfo.ArchetypeChunkCalculationIndicator == CalculationIndicator.Free)
                {
                    return;
                }

                var chunk = noiseMetaInfo.ArchetypeChunk;
                var noiseCalculations = chunk.GetBufferAccessor(NoiseCalculationType);
                for (var i = 0; i < noiseMetaInfo.ArchetypeChunkNoiseMetaInfoSize; i++)
                {
                    var archetypeChunkNoiseMetaInfo = noiseMetaInfo[i];
                    if (archetypeChunkNoiseMetaInfo.CalculationIndicator == CalculationIndicator.Free)
                    {
                        continue;
                    }

                    for (int j = archetypeChunkNoiseMetaInfo.StartingIndex, fraction = 0;
                        j <= archetypeChunkNoiseMetaInfo.EndingIndex;
                        j++, fraction++)
                    {
                        var noiseCalculation = (NoiseCalculation*) noiseCalculations[j].GetUnsafePtr();
                        var fromIndex = archetypeChunkNoiseMetaInfo.CurrentFraction(fraction) * PartSize;
                        for (var k = 0; k < Size; k++)
                        {
                            var currentIndex = fromIndex + k * 4;
                            noiseCalculation[k] = Noise.CalculateNoise(
                                Location.NextIndexes(currentIndex),
                                archetypeChunkNoiseMetaInfo.PositionOffset,
                                MeshAbout
                            );
                        }
                    }
                }
            }
        }

        #endregion

        public void Run(
            in ArchetypeChunkBufferType<NoiseCalculation> noiseCalculationType,
            in NativeArray<NoiseMetaInfo> noiseMetaInfos,
            MeshAbout meshAbout,
            int size,
            int partSize,
            ref JobHandle jobHandle
        )
        {
            var noiseHandlingJob = new NoiseHandling
            {
                NoiseCalculationType = noiseCalculationType,
                NoiseMetaInfos = noiseMetaInfos,
                MeshAbout = meshAbout,
                Size = size,
                PartSize = partSize
            };
            jobHandle = noiseHandlingJob.Schedule(noiseMetaInfos.Length, 1, jobHandle);
        }
    }
}