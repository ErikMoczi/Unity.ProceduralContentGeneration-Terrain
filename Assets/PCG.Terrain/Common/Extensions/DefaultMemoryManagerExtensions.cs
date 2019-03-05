using PCG.Terrain.Common.Collections.Unsafe;
using PCG.Terrain.Common.Helpers;
using Unity.Collections.LowLevel.Unsafe;

namespace PCG.Terrain.Common.Extensions
{
    public static class DefaultMemoryManagerExtensions
    {
        public static unsafe void* Allocate<M, C, T>(this M memoryManager, int size, MemoryOptions options)
            where M : struct, IDefaultMemoryManager
            where C : struct
            where T : struct
        {
            UnsafeCollectionsHelpers.IsBlittableAndThrow<C, T>();

            var totalSize = size * UnsafeUtility.SizeOf<T>();
            UnsafeCollectionsHelpers.IsCorrectSizeAndThrow<C>(totalSize);

            var buffer = UnsafeUtility.Malloc(
                totalSize,
                UnsafeUtility.AlignOf<T>(),
                memoryManager.Allocator
            );

            if ((options & MemoryOptions.ClearMemory) == MemoryOptions.ClearMemory)
            {
                UnsafeUtility.MemClear(buffer, totalSize);
            }

            return buffer;
        }

        public static unsafe void Free<M, C>(this M memoryManager, ref void* buffer)
            where M : struct, IDefaultMemoryManager
            where C : struct
        {
            UnsafeCollectionsHelpers.IsValidAllocatorAndThrow<C>(memoryManager.Allocator);
            UnsafeUtility.Free(buffer, memoryManager.Allocator);
            buffer = null;
        }

        public static unsafe void Resize<M, C, T>(this M memoryManager, ref void* buffer, int newSize, int oldSize)
            where M : struct, IDefaultMemoryManager
            where C : struct
            where T : struct
        {
            var oldBuffer = buffer;
            buffer = memoryManager.Allocate<M, C, T>(newSize, MemoryOptions.UninitializedMemory);

            UnsafeUtility.MemCpy(buffer, oldBuffer, oldSize * UnsafeUtility.SizeOf<T>());
            UnsafeUtility.Free(oldBuffer, memoryManager.Allocator);
        }
    }
}