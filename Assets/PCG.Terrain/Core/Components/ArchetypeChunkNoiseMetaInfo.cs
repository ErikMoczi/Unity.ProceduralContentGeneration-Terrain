using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using PCG.Terrain.Core.DataTypes;
using Unity.Entities;
using Unity.Mathematics;

namespace PCG.Terrain.Core.Components
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ArchetypeChunkNoiseMetaInfo : IBufferElementData
    {
        public Entity Entity;
        public int StartingIndex;
        public int EndingIndex;
        public int StartingFraction;
        public int2 PositionOffset;
        public CalculationIndicator CalculationIndicator;

        public int EntitiesCount => EndingIndex - StartingIndex + 1;
        public int EndingFraction => StartingFraction + EntitiesCount;

        public int CurrentFraction(int index)
        {
            CheckFraction(StartingFraction + index);
            return StartingFraction + index;
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private void CheckFraction(int index)
        {
            if (index > EndingFraction || index < StartingFraction)
            {
                throw new IndexOutOfRangeException(
                    $"Index {index} is out of range of '{StartingFraction}' - '{EndingFraction}' Fraction.");
            }
        }
    }
}