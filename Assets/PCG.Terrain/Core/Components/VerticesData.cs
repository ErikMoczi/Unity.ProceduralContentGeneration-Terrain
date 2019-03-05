using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;

namespace PCG.Terrain.Core.Components
{
    public struct VerticesData : IBufferElementData
    {
        private float2 _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator VerticesData(float2 value)
        {
            return new VerticesData {_value = value};
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float2(VerticesData verticesData)
        {
            return verticesData._value;
        }
    }
}