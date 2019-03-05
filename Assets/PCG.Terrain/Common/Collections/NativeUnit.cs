using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using PCG.Terrain.Common.Collections.Unsafe;
using PCG.Terrain.Common.Extensions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace PCG.Terrain.Common.Collections
{
    [NativeContainer]
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("Value = {" + nameof(Value) + "}")]
    public unsafe struct NativeUnit<T> : IUnsafeCollections
        where T : struct
    {
        private MemoryManager<NativeUnit<T>> _memoryManager;
        [NativeDisableUnsafePtrRestriction] private void* _buffer;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private AtomicSafetyHandle m_Safety;
#endif

        private ref T Buffer => ref UnsafeUtilityEx.AsRef<T>(_buffer);

        public NativeUnit(Allocator allocator, MemoryOptions options = MemoryOptions.ClearMemory)
        {
            _memoryManager = default;
            _buffer = _memoryManager.Init<T>(1, allocator
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                , out m_Safety
#endif
                , options
            );
        }

        public bool IsCreated => (IntPtr) _buffer != IntPtr.Zero;

        public T Value
        {
            get
            {
                this.IsAllocatedAndThrow();
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
                return Buffer;
            }
            [WriteAccessRequired]
            set
            {
                this.IsAllocatedAndThrow();
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
                Buffer = value;
            }
        }

        [WriteAccessRequired]
        public void Dispose()
        {
            this.IsAllocatedAndThrow();
            if (IsCreated)
            {
                _memoryManager.Free<MemoryManager<NativeUnit<T>>, NativeUnit<T>>(ref _buffer);
                _memoryManager.Dispose(
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    ref m_Safety
#endif
                );
            }
        }
    }
}