using PCG.Terrain.Core.Systems;
using PCG.Terrain.Performance.TestCase.Base;
using PCG.Terrain.Settings;

namespace PCG.Terrain.Performance.TestCase
{
    public sealed class TestCreateInit : PerformanceCreator
    {
        public TestCreateInit(ITerrainSettings terrainSettings) : base(terrainSettings)
        {
        }

        protected override void DefineRunSystems(IEcsSystemProxy system)
        {
            base.DefineRunSystems(system);
            system.Init<CreateSystem>(false, constructorArguments: TerrainSettings);
        }
    }

    public sealed class TestCreateRun : PerformanceCreator
    {
        public TestCreateRun(ITerrainSettings terrainSettings) : base(terrainSettings)
        {
        }

        protected override void DefineSetUpSystems(IEcsSystemProxy system)
        {
            base.DefineSetUpSystems(system);
            system.Init<CreateSystem>(false, constructorArguments: TerrainSettings);
        }

        protected override void DefineRunSystems(IEcsSystemProxy system)
        {
            base.DefineRunSystems(system);
            system.Get<CreateSystem>();
        }
    }
}