using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using PCG.Terrain.Common.Extensions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using NativeArrayExtensions = PCG.Terrain.Common.Extensions.NativeArrayExtensions;

namespace PCG.Terrain.Common.Collections.Unsafe
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct UnsafeArrayListData
    {
        [NativeDisableUnsafePtrRestriction] public void* Buffer;
        public int Length;
        public int Capacity;
    }

    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("Length = {" + nameof(Length) + "}")]
    [DebuggerTypeProxy(typeof(UnsafeArrayListDebugView<>))]
    public unsafe struct UnsafeArrayList<T> : IUnsafeCollections
        where T : struct
    {
        private MemoryManagerUnsafe<UnsafeArrayList<T>> _memoryManager;
        [NativeDisableUnsafePtrRestriction] private void* _arrayListData;

        private ref UnsafeArrayListData UnsafeArrayListData => ref *(UnsafeArrayListData*) _arrayListData;

        public UnsafeArrayList(int capacity, Allocator allocator, MemoryOptions options = MemoryOptions.ClearMemory)
        {
            _memoryManager = default;
            _arrayListData = _memoryManager.Init<UnsafeArrayListData>(1, allocator, options);

            UnsafeArrayListData.Buffer = (UnsafeArrayListData*) _memoryManager
                .Allocate<MemoryManagerUnsafe<UnsafeArrayList<T>>, UnsafeArrayList<T>, T>(capacity, options);
            UnsafeArrayListData.Length = 0;
            UnsafeArrayListData.Capacity = capacity;
        }

        public UnsafeArrayList(Allocator allocator, MemoryOptions options) : this(0, allocator, options)
        {
        }

        public UnsafeArrayList(Allocator allocator) : this(0, allocator)
        {
        }

        public bool IsCreated => (IntPtr) _arrayListData != IntPtr.Zero;

        public T this[int index]
        {
            get
            {
                this.IsAllocatedAndThrow();
                FailOutOfRangeError(index);
                return UnsafeUtility.ReadArrayElement<T>(UnsafeArrayListData.Buffer, index);
            }
            set
            {
                this.IsAllocatedAndThrow();
                FailOutOfRangeError(index);
                UnsafeUtility.WriteArrayElement(UnsafeArrayListData.Buffer, index, value);
            }
        }

        public int Length
        {
            get
            {
                this.IsAllocatedAndThrow();
                return UnsafeArrayListData.Length;
            }
            private set
            {
                this.IsAllocatedAndThrow();
                UnsafeArrayListData.Length = value;
            }
        }

        public int Capacity
        {
            get
            {
                this.IsAllocatedAndThrow();
                return UnsafeArrayListData.Capacity;
            }
            set
            {
                this.IsAllocatedAndThrow();
                FailOutCapacityError(value);
                if (UnsafeArrayListData.Capacity == value)
                {
                    return;
                }

                _memoryManager.Resize<MemoryManagerUnsafe<UnsafeArrayList<T>>, UnsafeArrayList<T>, T>(
                    ref UnsafeArrayListData.Buffer,
                    value,
                    Length
                );
                UnsafeArrayListData.Capacity = value;
            }
        }

        public void Add(T element)
        {
            var newLength = Length + 1;
            if (newLength > Capacity)
            {
                Capacity = newLength;
            }

            this[Length++] = element;
        }

        public void CopyFrom(void* elements, int count)
        {
            Length = 0;
            Capacity = count;
            UnsafeUtility.MemCpy(UnsafeArrayListData.Buffer, elements, UnsafeUtility.SizeOf<T>() * count);
            Length = count;
        }

        public void RemoveAtSwapBack(int index)
        {
            FailOutOfRangeError(index);
            var newLength = Length - 1;
            this[index] = this[newLength];
            Length = newLength;
        }

        public void* GetUnsafePtr()
        {
            this.IsAllocatedAndThrow();
            return UnsafeArrayListData.Buffer;
        }

        public NativeArray<T> AsNativeArray()
        {
            this.IsAllocatedAndThrow();
            return NativeArrayExtensions.Impostor<T>(UnsafeArrayListData.Buffer, Length);
        }

        public void Clear()
        {
            Length = 0;
            Capacity = 0;
        }

        public void Dispose()
        {
            this.IsAllocatedAndThrow();
            if (IsCreated)
            {
                _memoryManager.Free<MemoryManagerUnsafe<UnsafeArrayList<T>>, UnsafeArrayList<T>>(
                    ref UnsafeArrayListData.Buffer
                );
                _memoryManager.Free<MemoryManagerUnsafe<UnsafeArrayList<T>>, UnsafeArrayList<T>>(ref _arrayListData);
                _memoryManager.Dispose();
            }
        }

        #region Helpers

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private void FailOutOfRangeError(int index)
        {
            if (index < 0 || index >= Length)
            {
                throw new IndexOutOfRangeException(
                    $"Index {index} used in {typeof(UnsafeArrayList<T>)} is out of range of '{Length}'."
                );
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private void FailOutCapacityError(int capacity)
        {
            if (capacity < Length)
            {
                throw new ArgumentException(
                    $"Capacity ({capacity}) used in {typeof(UnsafeArrayList<T>)} must be larger than the Length ({Length}).",
                    nameof(capacity)
                );
            }
        }

        #endregion
    }

    public sealed class UnsafeArrayListDebugView<T>
        where T : struct
    {
        private UnsafeArrayList<T> _unsafeArrayList;

        public UnsafeArrayListDebugView(UnsafeArrayList<T> unsafeArrayList)
        {
            _unsafeArrayList = unsafeArrayList;
        }

        public T[] Items => _unsafeArrayList.AsNativeArray().ToArray();
    }
}