using System;

namespace PCG.Terrain.Common.Collections.Unsafe
{
    public interface IUnsafeCollections : IDisposable
    {
        bool IsCreated { get; }
    }
}