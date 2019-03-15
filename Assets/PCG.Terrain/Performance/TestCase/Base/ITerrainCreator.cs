namespace PCG.Terrain.Performance.TestCase.Base
{
    public interface ITerrainCreator
    {
        void SetUp();
        void Run();
        void CleanUp();
    }
}