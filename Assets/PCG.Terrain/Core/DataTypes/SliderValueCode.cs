using System;

namespace PCG.Terrain.Core.DataTypes
{
    [Flags]
    public enum SliderValueCode : byte
    {
        Resolution = 0x00,
        ChunkCount = 0x01,
        ChunksPerFrame = 0x02,
        ChunkThreshold = 0x03,
        NoiseFrequency = 0x04,
        NoiseOctavec = 0x05,
        NoiseLacunarity = 0x06,
        NoisePersistance = 0x07,
        NoiseAmplitude = 0x08
    }
}