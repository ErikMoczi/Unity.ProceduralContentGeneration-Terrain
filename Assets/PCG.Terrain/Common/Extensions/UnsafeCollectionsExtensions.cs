using System;
using System.Diagnostics;
using PCG.Terrain.Common.Collections.Unsafe;

namespace PCG.Terrain.Common.Extensions
{
    public static class UnsafeCollectionsExtensions
    {
        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        public static void IsAllocatedAndThrow<Container>(this Container container)
            where Container : struct, IUnsafeCollections
        {
            if (!container.IsCreated)
            {
                throw new InvalidOperationException(
                    $"The {typeof(Container)} has yet to not been allocated or has been deallocated!"
                );
            }
        }
    }
}