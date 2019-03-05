using System;
using System.Collections.Generic;
using System.Diagnostics;
using PCG.Terrain.Common.Helpers;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace PCG.Terrain.Common.Extensions
{
    public static class ListExtensions
    {
        public static unsafe void* GetUnsafePtr<T>(this List<T> list, out ulong gcHandle)
            where T : struct
        {
            return UnsafeUtility.PinGCArrayAndGetDataAddress(
                NoAllocHelpers.ExtractArrayFromListT(list),
                out gcHandle
            );
        }

        public static unsafe void NativeInject<T, U>(this List<T> list, NativeArray<U> nativeArray)
            where T : struct
            where U : struct
        {
            CheckSameDataType<T, U>();
            NativeInject(list, 0, nativeArray.GetUnsafePtr(), 0, nativeArray.Length);
        }

        public static unsafe void NativeInject<T, U>(this List<T> list, NativeList<U> nativeList)
            where T : struct
            where U : struct
        {
            CheckSameDataType<T, U>();
            NativeInject(list, 0, nativeList.GetUnsafePtr(), 0, nativeList.Length);
        }

        public static unsafe void NativeInject<T, U>(this List<T> list, NativeSlice<U> nativeSlice)
            where T : struct
            where U : struct
        {
            CheckSameDataType<T, U>();
            NativeInject(list, 0, nativeSlice.GetUnsafePtr(), 0, nativeSlice.Length);
        }

        public static unsafe void NativeInject<T, U>(this List<T> list, DynamicBuffer<U> buffer)
            where T : struct
            where U : struct
        {
            CheckSameDataType<T, U>();
            NativeInject(list, 0, buffer.GetUnsafePtr(), 0, buffer.Length);
        }

        private static unsafe void NativeInject<T>(this List<T> list, int startIndex, void* src, int srcIndex,
            int length)
            where T : struct
        {
            var newLength = startIndex + length;
            if (list.Capacity < newLength)
            {
                list.Capacity = newLength;
            }

            var size = UnsafeUtility.SizeOf<T>();
            var dst = list.GetUnsafePtr(out var gcHandle);

            UnsafeUtility.MemCpy(
                (void*) ((IntPtr) dst + startIndex * size),
                (void*) ((IntPtr) src + srcIndex * size),
                length * size
            );

            UnsafeUtility.ReleaseGCObject(gcHandle);
            NoAllocHelpers.ResizeList(list, newLength);
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void CheckSameDataType<T, U>()
            where T : struct
            where U : struct
        {
            if (UnsafeUtility.SizeOf<U>() != UnsafeUtility.SizeOf<T>())
            {
                throw new InvalidOperationException(
                    $"Types {typeof(U)} and {typeof(T)} are of different sizes; cannot transfer data"
                );
            }
        }
    }
}