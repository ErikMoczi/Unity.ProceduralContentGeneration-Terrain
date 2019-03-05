using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace PCG.Terrain.Core.DataTypes
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Offset : IEquatable<Offset>
    {
        private int2 _value;
        private int _hash;

        public int2 Value
        {
            get => _value;
            set
            {
                _value = value;
                ReCalculateHash();
            }
        }

        public static implicit operator int2(Offset groupId)
        {
            return groupId.Value;
        }

        public static implicit operator Offset(int2 value)
        {
            return new Offset {Value = value};
        }

        public bool Equals(Offset other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Offset other && Equals(other);
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return _hash;
        }

        public override string ToString()
        {
            return $"{nameof(Offset)}: {_value}";
        }

        private void ReCalculateHash()
        {
            _hash = _value.GetHashCode();
        }
    }
}