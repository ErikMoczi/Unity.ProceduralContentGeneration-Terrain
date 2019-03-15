using System;
using UnityEngine;

namespace PCG.Terrain.Settings
{
    public interface ITerrainSettings : ICloneable
    {
        int Resolution { get; }
        int ChunkCount { get; }
        int ChunksPerFrame { get; }
        int ChangeThreshold { get; }
        NoiseSettings NoiseSettings { get; }
        MeshSettings MeshSettings { get; }
        int TotalVertices { get; }
        int TotalTriangles { get; }
        int ArrayChunk { get; }
        Texture2D GradientHeight();
    }

    public interface ITerrainSettingsImpostor : ITerrainSettings
    {
        new int Resolution { get; set; }
        new int ChunkCount { get; set; }
        new int ChunksPerFrame { get; set; }
        new int ChangeThreshold { get; set; }
        ref NoiseSettingsImpostor NoiseSettingsImpostor { get; }
    }
}