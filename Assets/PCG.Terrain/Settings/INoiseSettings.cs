namespace PCG.Terrain.Settings
{
    public interface INoiseSettings
    {
        float Frequency { get; }
        int Octaves { get; }
        float Lacunarity { get; }
        float Persistence { get; }
    }
}