using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using PCG.Terrain.Common.Enumerators;
using PCG.Terrain.Common.Helpers;
using Unity.Collections;
using UnityEngine;

namespace PCG.Terrain.Common.Extensions
{
    public static class NativeHashMapExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeHashMapEnumerator<TKey, TValue> GetEnumerator<TKey, TValue>(
            this NativeHashMap<TKey, TValue> nativeHashMap)
            where TKey : struct, IEquatable<TKey>
            where TValue : struct
        {
            return new NativeHashMapEnumerator<TKey, TValue>(in nativeHashMap);
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        public static void FailOnDuplicateData<TKey, TValue>(this NativeHashMap<TKey, TValue> nativeHashMap)
            where TKey : struct, IEquatable<TKey>
            where TValue : struct
        {
            var length = nativeHashMap.Length;
            var data = new TValue[length];
            using (var actualPositions = nativeHashMap.GetEnumerator())
            {
                var i = 0;
                while (actualPositions.MoveNext())
                {
                    var item = actualPositions.Current;
                    data[i] = item.Value;
                    i++;
                }
            }

            if (data.Distinct().Count() != data.Length)
            {
                throw new UnityException($"{nativeHashMap.GetType()} has duplicate values");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TryGetAndThrowOnFail<TKey, TValue>(this NativeHashMap<TKey, TValue> nativeHashMap, TKey key,
            out TValue item)
            where TKey : struct, IEquatable<TKey>
            where TValue : struct
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var state = nativeHashMap.TryGetValue(key, out item);
            HashMapHelpers.ThrowOnFailedKey(state, key);
#else
            nativeHashMap.TryGetValue(key, out item);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TryAddAndThrowOnFail<TKey, TValue>(this NativeHashMap<TKey, TValue> nativeHashMap, TKey key,
            TValue item)
            where TKey : struct, IEquatable<TKey>
            where TValue : struct
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var state = nativeHashMap.TryAdd(key, item);
            HashMapHelpers.ThrowOnFailedKey(state, key);
#else
            nativeHashMap.TryAdd(key, item);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TryAddAndThrowOnFail<TKey, TValue>(this NativeHashMap<TKey, TValue>.Concurrent nativeHashMap,
            TKey key, TValue item)
            where TKey : struct, IEquatable<TKey>
            where TValue : struct
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var state = nativeHashMap.TryAdd(key, item);
            HashMapHelpers.ThrowOnFailedKey(state, key);
#else
            nativeHashMap.TryAdd(key, item);
#endif
        }
    }
}