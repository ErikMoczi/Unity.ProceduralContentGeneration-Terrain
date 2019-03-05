using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PCG.Terrain.Common.Impostors;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace PCG.Terrain.Common.Enumerators
{
    public unsafe struct NativeMultiHashMapEnumerator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>
        where TKey : struct, IEquatable<TKey>
        where TValue : struct
    {
        private readonly NativeMultiHashMapImpostor<TKey, TValue> _nativeMultiHashMapImpostor;
        private readonly byte* _values;
        private readonly byte* _keys;
        private readonly int* _next;
        private readonly int* _buckets;

        private int _index;
        private int _entryIndex;

        public NativeMultiHashMapEnumerator(in NativeMultiHashMap<TKey, TValue> nativeMultiHashMap)
        {
            _nativeMultiHashMapImpostor = nativeMultiHashMap;

            _values = _nativeMultiHashMapImpostor.m_Buffer->values;
            _keys = _nativeMultiHashMapImpostor.m_Buffer->keys;
            _next = (int*) _nativeMultiHashMapImpostor.m_Buffer->next;
            _buckets = (int*) _nativeMultiHashMapImpostor.m_Buffer->buckets;

            _index = 0;
            _entryIndex = -1;
        }

        public KeyValuePair<TKey, TValue> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new KeyValuePair<TKey, TValue>(
                UnsafeUtility.ReadArrayElement<TKey>(_keys, _entryIndex),
                UnsafeUtility.ReadArrayElement<TValue>(_values, _entryIndex)
            );
        }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            var length = _nativeMultiHashMapImpostor.m_Buffer->bucketCapacityMask + 1;
            for (; _index < length; _index++)
            {
                _entryIndex = _entryIndex == -1 ? _buckets[_index] : _next[_entryIndex];
                if (_entryIndex != -1)
                {
                    return true;
                }
            }

            return false;
        }

        public void Reset()
        {
            _index = 0;
            _entryIndex = -1;
        }

        public void Dispose()
        {
        }
    }
}