using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace PCG.Terrain.Common.Impostors
{
    [StructLayout(LayoutKind.Sequential)]
    [NativeContainer]
    public unsafe struct NativeHashMapImpostor<TKey, TValue>
        where TKey : struct, IEquatable<TKey>
        where TValue : struct
    {
        [NativeDisableUnsafePtrRestriction] public NativeHashMapDataImpostor* m_Buffer;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle m_Safety;
        [NativeSetClassTypeToNullOnSchedule] DisposeSentinel m_DisposeSentinel;
#endif

        Allocator m_AllocatorLabel;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeHashMapImpostor<TKey, TValue>(NativeHashMap<TKey, TValue> nativeHashMap)
        {
            var ptr = UnsafeUtility.AddressOf(ref nativeHashMap);
            UnsafeUtility.CopyPtrToStructure(ptr, out NativeHashMapImpostor<TKey, TValue> impostor);
            return impostor;
        }
    }
}