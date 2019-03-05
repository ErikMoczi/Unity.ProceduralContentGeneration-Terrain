using System;
using System.Diagnostics;

namespace PCG.Terrain.Common.Helpers
{
    public static class HashMapHelpers
    {
        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        public static void ThrowOnFailedKey<TKey>(bool state, TKey key)
            where TKey : struct, IEquatable<TKey>
        {
            if (!state)
            {
                throw new ArgumentException($"Trying to access invalid key: {key}", $"{nameof(key)}");
            }
        }
    }
}