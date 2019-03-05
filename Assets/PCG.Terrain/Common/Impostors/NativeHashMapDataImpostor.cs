using System.Runtime.InteropServices;
using Unity.Jobs.LowLevel.Unsafe;

namespace PCG.Terrain.Common.Impostors
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct NativeHashMapDataImpostor
    {
        public byte* values;
        public byte* keys;
        public byte* next;
        public byte* buckets;
        public int keyCapacity;
        public int bucketCapacityMask;
        private fixed byte padding1[60];
        public fixed int firstFreeTLS[JobsUtility.MaxJobThreadCount * IntsPerCacheLine];
        public int allocatedIndexLength;
        public const int IntsPerCacheLine = JobsUtility.CacheLineSize / sizeof(int);
    }
}