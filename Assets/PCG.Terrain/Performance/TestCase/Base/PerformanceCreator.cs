using PCG.Terrain.Core.Systems;
using PCG.Terrain.Settings;

namespace PCG.Terrain.Performance.TestCase.Base
{
    public class PerformanceCreator : TerrainCreator
    {
        public PerformanceCreator(ITerrainSettings terrainSettings) : base(terrainSettings)
        {
        }

        protected override void DefineCleanupSystems(IEcsSystemProxy system)
        {
            base.DefineCleanupSystems(system);
            system.Init<CleanUpSystem>();
        }
    }
}