using System;
using Unity.Entities;
using Unity.Mathematics;

namespace PCG.Terrain.Tests
{
    public interface IGenerateData<TKey, TValue>
        where TKey : struct, IEquatable<TKey>
        where TValue : struct
    {
        TKey GetKey(int index);
        TValue GetValue(int index);
    }

    public abstract class GenerateData<TKey, TValue> : IGenerateData<TKey, TValue>
        where TKey : struct, IEquatable<TKey>
        where TValue : struct
    {
        public abstract TKey GetKey(int index);
        public abstract TValue GetValue(int index);
    }

    public sealed class Generator1 : GenerateData<int, int>
    {
        public override int GetKey(int index)
        {
            return index;
        }

        public override int GetValue(int index)
        {
            return index ^ 2;
        }
    }

    public sealed class Generator2 : GenerateData<int2, int>
    {
        public override int2 GetKey(int index)
        {
            return math.int2(index);
        }

        public override int GetValue(int index)
        {
            return index ^ 2;
        }
    }

    public sealed class Generator3 : GenerateData<Entity, float4>
    {
        public override Entity GetKey(int index)
        {
            return new Entity {Index = index, Version = index + index};
        }

        public override float4 GetValue(int index)
        {
            return math.float4(index / 2f);
        }
    }

    public sealed class Generator4 : GenerateData<double2, int>
    {
        public override double2 GetKey(int index)
        {
            return math.double2((index ^ 2) / 2d);
        }

        public override int GetValue(int index)
        {
            return index;
        }
    }

    public sealed class Generator5 : GenerateData<float4, Entity>
    {
        public override float4 GetKey(int index)
        {
            return math.float4(index / 2f);
        }

        public override Entity GetValue(int index)
        {
            return new Entity {Index = index, Version = index + index};
        }
    }
}