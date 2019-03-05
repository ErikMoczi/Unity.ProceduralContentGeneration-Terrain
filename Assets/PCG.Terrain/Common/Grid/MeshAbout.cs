using PCG.Terrain.Settings;

namespace PCG.Terrain.Common.Grid
{
    public struct MeshAbout
    {
        public int Size { get; }
        public float StepSize { get; }
        public NoiseSettings NoiseSettings { get; }

        public MeshAbout(int size, NoiseSettings noiseSettings)
        {
            Size = size;
            StepSize = 1f / size;
            NoiseSettings = noiseSettings;
        }
    }
}