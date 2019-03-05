using System;
using System.Runtime.InteropServices;
using PCG.Terrain.Common.Extensions;
using PCG.Terrain.Common.Helpers;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace PCG.Terrain.Common.Collections.Unsafe
{
    [Flags]
    public enum MemoryOptions : byte
    {
        UninitializedMemory = 0x00,
        ClearMemory = 0x01
    }

    public interface IDefaultMemoryManager
#if !ENABLE_UNITY_COLLECTIONS_CHECKS
        : IDisposable
#endif
    {
        Allocator Allocator { get; }
    }

    public interface IMemoryManager : IDefaultMemoryManager
    {
        unsafe void* Init<T>(int size, Allocator allocator
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            , out AtomicSafetyHandle atomicSafetyHandle
#endif
            , MemoryOptions options
        ) where T : struct;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        void Dispose(ref AtomicSafetyHandle atomicSafetyHandle);
#endif
    }

    public interface IMemoryManagerUnsafe : IDefaultMemoryManager
    {
        unsafe void* Init<T>(int size, Allocator allocator, MemoryOptions options) where T : struct;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        void Dispose();
#endif
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MemoryManagerUnsafe<Container> : IMemoryManagerUnsafe
        where Container : struct
    {
        private Allocator _allocator;
        public Allocator Allocator => _allocator;

        public unsafe void* Init<T>(int size, Allocator allocator, MemoryOptions options)
            where T : struct
        {
            UnsafeCollectionsHelpers.IsAlreadyCreatedAndThrow<Container>(_allocator);
            UnsafeCollectionsHelpers.IsCorrectAllocatorAndThrow<Container>(allocator);

            _allocator = allocator;

            return this.Allocate<MemoryManagerUnsafe<Container>, Container, T>(size, options);
        }

        public void Dispose()
        {
            UnsafeCollectionsHelpers.DisposeAllocator<Container>(ref _allocator);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MemoryManager<Container> : IMemoryManager
        where Container : struct
    {
        private Allocator _allocator;
        public Allocator Allocator => _allocator;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private AtomicSafetyHandle _atomicSafetyHandle;

        [NativeSetClassTypeToNullOnSchedule] private DisposeSentinel _disposeSentinel;
#endif

        public unsafe void* Init<T>(int size, Allocator allocator
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            , out AtomicSafetyHandle atomicSafetyHandle
#endif
            , MemoryOptions options
        )
            where T : struct
        {
            UnsafeCollectionsHelpers.IsAlreadyCreatedAndThrow<Container>(_allocator);
            UnsafeCollectionsHelpers.IsCorrectAllocatorAndThrow<Container>(allocator);

            _allocator = allocator;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Create(out atomicSafetyHandle, out _disposeSentinel, 0, allocator);
            _atomicSafetyHandle = atomicSafetyHandle;
#endif

            return this.Allocate<MemoryManager<Container>, Container, T>(size, options);
        }

        public void Dispose(
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            ref AtomicSafetyHandle atomicSafetyHandle
#endif
        )
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            UnsafeCollectionsHelpers.IsValidAllocatorAndThrow<Container>(Allocator);
            DisposeSentinel.Dispose(ref atomicSafetyHandle, ref _disposeSentinel);
#endif
            UnsafeCollectionsHelpers.DisposeAllocator<Container>(ref _allocator);
        }
    }
}