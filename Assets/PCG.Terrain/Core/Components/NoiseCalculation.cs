using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;

namespace PCG.Terrain.Core.Components
{
    [InternalBufferCapacity(Environment.ValuesPerEntity)]
    public struct NoiseCalculation : IBufferElementData
    {
        private float4 _values;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NoiseCalculation(float4 values)
        {
            return new NoiseCalculation {_values = values};
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float4(NoiseCalculation noiseCalculation)
        {
            return noiseCalculation._values;
        }
    }
}