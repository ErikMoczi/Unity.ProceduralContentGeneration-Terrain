using System;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace PCG.Terrain.Common.Helpers
{
    public static class UnsafeCollectionsHelpers
    {
        public static void DisposeAllocator<Container>(ref Allocator allocator)
            where Container : struct
        {
            IsValidAllocatorAndThrow<Container>(allocator);
            allocator = Allocator.Invalid;
        }

        [BurstDiscard]
        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        public static void IsBlittableAndThrow<Container, T>()
            where T : struct
            where Container : struct
        {
            if (!UnsafeUtility.IsBlittable<T>())
            {
                throw new ArgumentException(
                    $"{typeof(T)} used in {typeof(Container)} must be blittable"
                );
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        public static void IsCorrectAllocatorAndThrow<Container>(Allocator allocator)
            where Container : struct
        {
            if (allocator <= Allocator.None)
            {
                throw new ArgumentException(
                    $"{allocator} used in {typeof(Container)} must be Temp, TempJob or Persistent",
                    nameof(allocator)
                );
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        public static void IsCorrectSizeAndThrow<Container>(int size)
            where Container : struct
        {
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    $"Length used in {typeof(Container)} must be >= 0"
                );
            }

            if (size > (long) int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    $"Capacity * sizeof(T) used in {typeof(Container)} cannot exceed {int.MaxValue} bytes");
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        public static void IsAlreadyCreatedAndThrow<Container>(Allocator allocator)
            where Container : struct
        {
            if (allocator != Allocator.Invalid)
            {
                throw new InvalidOperationException(
                    $"The {typeof(Container)} is already created."
                );
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        public static void IsValidAllocatorAndThrow<Container>(Allocator allocator)
            where Container : struct
        {
            if (!UnsafeUtility.IsValidAllocator(allocator))
            {
                throw new InvalidOperationException(
                    $"The {typeof(Container)} can not be Disposed because it was not allocated with a valid allocator."
                );
            }
        }
    }
}