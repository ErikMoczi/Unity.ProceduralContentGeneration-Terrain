using System.Runtime.CompilerServices;
using PCG.Terrain.Core.DataTypes;
using Unity.Entities;

namespace PCG.Terrain.Core.Components
{
    public struct ArchetypeChunkCalculationIndicator : IComponentData
    {
        private CalculationIndicator _calculationIndicator;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ArchetypeChunkCalculationIndicator(CalculationIndicator calculationIndicator)
        {
            return new ArchetypeChunkCalculationIndicator {_calculationIndicator = calculationIndicator};
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator CalculationIndicator(
            ArchetypeChunkCalculationIndicator archetypeChunkCalculationIndicator)
        {
            return archetypeChunkCalculationIndicator._calculationIndicator;
        }
    }
}