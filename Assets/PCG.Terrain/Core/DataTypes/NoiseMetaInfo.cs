using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using PCG.Terrain.Core.Components;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace PCG.Terrain.Core.DataTypes
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct NoiseMetaInfo
    {
        private readonly ArchetypeChunkCalculationIndicator* _archetypeChunkCalculationIndicator;
        private readonly ArchetypeChunkNoiseMetaInfo* _archetypeChunkNoiseMetaInfo;
        private readonly ChunkHeader* _chunkHeader;
        private readonly int _archetypeChunkNoiseMetaInfoSize;
        private readonly int _baseIndex;

        public NoiseMetaInfo(ArchetypeChunkCalculationIndicator* archetypeChunkCalculationIndicator,
            ArchetypeChunkNoiseMetaInfo* archetypeChunkNoiseMetaInfo, ChunkHeader* chunkHeader,
            int archetypeChunkNoiseMetaInfoSize, int baseIndex) : this()
        {
            _archetypeChunkCalculationIndicator = archetypeChunkCalculationIndicator;
            _archetypeChunkNoiseMetaInfo = archetypeChunkNoiseMetaInfo;
            _chunkHeader = chunkHeader;
            _archetypeChunkNoiseMetaInfoSize = archetypeChunkNoiseMetaInfoSize;
            _baseIndex = baseIndex;
        }

        public int ArchetypeChunkNoiseMetaInfoSize => _archetypeChunkNoiseMetaInfoSize;

        public ArchetypeChunk ArchetypeChunk
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => UnsafeUtility.ReadArrayElement<ChunkHeader>(
                _chunkHeader,
                _baseIndex
            ).ArchetypeChunk;
        }

        public CalculationIndicator ArchetypeChunkCalculationIndicator
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => UnsafeUtility.ReadArrayElement<ArchetypeChunkCalculationIndicator>(
                _archetypeChunkCalculationIndicator,
                _baseIndex
            );
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => UnsafeUtility.WriteArrayElement<ArchetypeChunkCalculationIndicator>(
                _archetypeChunkCalculationIndicator,
                _baseIndex,
                value
            );
        }

        public ref ArchetypeChunkNoiseMetaInfo this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                FailOutOfRangeError(index);
                return ref UnsafeUtilityEx.ArrayElementAsRef<ArchetypeChunkNoiseMetaInfo>(
                    _archetypeChunkNoiseMetaInfo,
                    index
                );
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private void FailOutOfRangeError(int index)
        {
            if (index < 0 || index > _archetypeChunkNoiseMetaInfoSize)
            {
                throw new IndexOutOfRangeException(
                    $"Index {index} is out of range of '{_archetypeChunkNoiseMetaInfoSize}'."
                );
            }
        }
    }
}