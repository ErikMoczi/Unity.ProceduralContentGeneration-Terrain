using Unity.Jobs;

namespace PCG.Terrain.Common.Jobs
{
    public interface IJobParallelForGrid : IJobParallelFor
    {
        int GridSize { get; }
    }
}