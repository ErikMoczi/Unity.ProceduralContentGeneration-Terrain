namespace PCG.Terrain.Generator.Base
{
    public interface ITerrainCreator
    {
        void SetUp();
        void Run();
        void CleanUp();
    }
}