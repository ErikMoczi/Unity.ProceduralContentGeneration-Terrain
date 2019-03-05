using UnityEngine;

namespace PCG.Terrain.Settings
{
    public interface ITerrainSettings
    {
        int Resolution { get; }
        int ChunkCount { get; }
        NoiseSettings NoiseSettings { get; }
        MeshSettings MeshSettings { get; }
        int TotalVertices { get; }
        int TotalTriangles { get; }
        int ChunksPerFrame { get; }
        int ChangeThreshold { get; }
        int ArrayChunk { get; }
        int GradientResolution { get; }
        Gradient Gradient { get; }
        Texture2D GradientHeight();
    }
}