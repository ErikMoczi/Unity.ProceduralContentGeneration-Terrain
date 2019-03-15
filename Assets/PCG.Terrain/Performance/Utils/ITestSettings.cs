using Unity.PerformanceTesting;

namespace PCG.Terrain.Performance.Utils
{
    public interface ITestSettings
    {
        int TotalRuns { get; }
        int WarmUpCount { get; }
        int ResultsPrecision { get; }
        SampleUnit SampleUnit { get; }
    }
}