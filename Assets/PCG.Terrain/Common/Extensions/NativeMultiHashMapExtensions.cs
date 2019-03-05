using System;
using System.Runtime.CompilerServices;
using PCG.Terrain.Common.Enumerators;
using PCG.Terrain.Common.Helpers;
using Unity.Collections;

namespace PCG.Terrain.Common.Extensions
{
    public static class NativeMultiHashMapExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeMultiHashMapEnumerator<TKey, TValue> GetEnumerator<TKey, TValue>(
            this NativeMultiHashMap<TKey, TValue> nativeMultiHashMap)
            where TKey : struct, IEquatable<TKey>
            where TValue : struct
        {
            return new NativeMultiHashMapEnumerator<TKey, TValue>(in nativeMultiHashMap);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TryGetFirstValueAndThrowOnFail<TKey, TValue>(
            this NativeMultiHashMap<TKey, TValue> nativeMultiHashMap, TKey key, out TValue item,
            out NativeMultiHashMapIterator<TKey> it)
            where TKey : struct, IEquatable<TKey>
            where TValue : struct
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var state = nativeMultiHashMap.TryGetFirstValue(key, out item, out it);
            HashMapHelpers.ThrowOnFailedKey(state, key);
#else
            nativeMultiHashMap.TryGetFirstValue(key, out item, out it);
#endif
        }
    }
}