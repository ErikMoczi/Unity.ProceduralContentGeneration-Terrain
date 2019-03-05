using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PCG.Terrain.Common.Extensions;
using PCG.Terrain.Common.Helpers;
using PCG.Terrain.Core.Components;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace PCG.Terrain.Common.Memory
{
    public static class VerticesDataHandling
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Collect(List<Vector3> list, int totalVertices, in DynamicBuffer<VerticesData> data)
        {
            list.Capacity = totalVertices;

            var dst = list.GetUnsafePtr(out var gcHandle);
            data.ExtractData(dst, totalVertices);
            UnsafeUtility.ReleaseGCObject(gcHandle);

            NoAllocHelpers.ResizeList(list, totalVertices);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void InjectData(this in DynamicBuffer<VerticesData> dynamicBuffer, void* src, int size)
        {
            Assert.AreEqual(dynamicBuffer.Length, size);

            var dst = dynamicBuffer.GetUnsafePtr();

            var memorySize1 = UnsafeUtility.SizeOf<float>();
            var memorySize2 = UnsafeUtility.SizeOf<float2>();
            var memorySize3 = UnsafeUtility.SizeOf<float3>();

            UnsafeUtility.MemCpyStride(
                dst, memorySize2,
                src, memorySize3,
                memorySize1, size
            );
            UnsafeUtility.MemCpyStride(
                (void*) ((IntPtr) dst + memorySize1), memorySize2,
                (void*) ((IntPtr) src + memorySize1 * 2), memorySize3,
                memorySize1, size
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ExtractData(this in DynamicBuffer<VerticesData> dynamicBuffer, void* dst, int size)
        {
            Assert.AreEqual(dynamicBuffer.Length, size);

            var src = dynamicBuffer.GetUnsafePtr();

            var memorySize1 = UnsafeUtility.SizeOf<float>();
            var memorySize2 = UnsafeUtility.SizeOf<float2>();
            var memorySize3 = UnsafeUtility.SizeOf<float3>();

            UnsafeUtility.MemCpyStride(
                dst, memorySize3,
                src, memorySize2,
                memorySize1, size
            );
            UnsafeUtility.MemCpyStride(
                (void*) ((IntPtr) dst + memorySize1 * 2), memorySize3,
                (void*) ((IntPtr) src + memorySize1), memorySize2,
                memorySize1, size
            );
        }
    }
}