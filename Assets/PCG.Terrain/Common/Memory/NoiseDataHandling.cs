using System;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace PCG.Terrain.Common.Memory
{
    public static class NoiseDataHandling
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void FillVector3Y(void* dst, void* src, int from, int size)
        {
            var memorySize1 = UnsafeUtility.SizeOf<float>();
            var memorySize3 = UnsafeUtility.SizeOf<float3>();

            UnsafeUtility.MemCpyStride(
                (void*) ((IntPtr) dst + memorySize3 * from + memorySize1), memorySize3,
                src, memorySize1,
                memorySize1,
                size
            );
        }
    }
}