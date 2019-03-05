using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using PCG.Terrain.Common.Collections.Unsafe;
using PCG.Terrain.Common.Extensions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace PCG.Terrain.Common.Collections
{
    [NativeContainer]
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("Length = {" + nameof(Count) + "}")]
    public unsafe struct NativeCounter : IUnsafeCollections
    {
        private MemoryManager<NativeCounter> _memoryManager;
        [NativeDisableUnsafePtrRestriction] void* m_Counter;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle m_Safety;
#endif

        private ref int Counter => ref UnsafeUtilityEx.AsRef<int>(m_Counter);

        public NativeCounter(Allocator allocator)
        {
            _memoryManager = default;
            m_Counter = _memoryManager.Init<int>(1, allocator
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                , out m_Safety
#endif
                , MemoryOptions.UninitializedMemory
            );

            Counter = 0;
        }

        public bool IsCreated => (IntPtr) m_Counter != IntPtr.Zero;

        [WriteAccessRequired]
        public void Increment()
        {
            this.IsAllocatedAndThrow();
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            Counter++;
        }

        public int Count
        {
            get
            {
                this.IsAllocatedAndThrow();
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
                return Counter;
            }
            [WriteAccessRequired]
            set
            {
                this.IsAllocatedAndThrow();
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
                Counter = value;
            }
        }

        [WriteAccessRequired]
        public void Dispose()
        {
            this.IsAllocatedAndThrow();
            if (IsCreated)
            {
                _memoryManager.Free<MemoryManager<NativeCounter>, NativeCounter>(ref m_Counter);
                _memoryManager.Dispose(
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    ref m_Safety
#endif
                );
            }
        }

        [WriteAccessRequired]
        public Concurrent ToConcurrent()
        {
            this.IsAllocatedAndThrow();
            Concurrent concurrent;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
            concurrent.m_Safety = m_Safety;
            AtomicSafetyHandle.UseSecondaryVersion(ref concurrent.m_Safety);
#endif

            concurrent.m_Counter = (int*) Counter;
            return concurrent;
        }

        [NativeContainer]
        [NativeContainerIsAtomicWriteOnly]
        public struct Concurrent
        {
            [NativeDisableUnsafePtrRestriction] internal int* m_Counter;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            internal AtomicSafetyHandle m_Safety;
#endif

            [WriteAccessRequired]
            public int Increment()
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
                return Interlocked.Increment(ref *m_Counter);
            }

            [WriteAccessRequired]
            public int Add(int value)
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
                return Interlocked.Add(ref *m_Counter, value);
            }
        }
    }
}