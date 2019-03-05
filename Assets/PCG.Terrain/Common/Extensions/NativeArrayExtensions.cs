using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace PCG.Terrain.Common.Extensions
{
    public static class NativeArrayExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe NativeArray<T> Impostor<T>(this ref NativeArray<T> nativeArray)
            where T : struct
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var nativeArrayImpostor = NativeArrayInvalid<T>(nativeArray.GetUnsafePtr(), nativeArray.Length);

            var atomicSafetyHandle = NativeArrayUnsafeUtility.GetAtomicSafetyHandle(nativeArray);
            AtomicSafetyHandle.CheckGetSecondaryDataPointerAndThrow(atomicSafetyHandle);
            var secondaryAtomicSafetyHandle = atomicSafetyHandle;
            AtomicSafetyHandle.UseSecondaryVersion(ref secondaryAtomicSafetyHandle);

            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(
                ref nativeArrayImpostor,
                secondaryAtomicSafetyHandle
            );

            return nativeArrayImpostor;
#else
            return nativeArray;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe NativeArray<T> Impostor<T>(void* buffer, int size)
            where T : struct
        {
            var nativeArrayImpostor = NativeArrayInvalid<T>(buffer, size);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(
                ref nativeArrayImpostor,
                AtomicSafetyHandle.Create()
            );
#endif

            return nativeArrayImpostor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe NativeArray<T> NativeArrayInvalid<T>(void* buffer, int size)
            where T : struct
        {
            return NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(
                buffer,
                size,
                Allocator.Invalid
            );
        }
    }
}