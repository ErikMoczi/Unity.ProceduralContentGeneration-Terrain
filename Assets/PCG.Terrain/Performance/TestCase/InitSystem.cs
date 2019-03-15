using PCG.Terrain.Core.Systems;
using PCG.Terrain.Performance.TestCase.Base;
using PCG.Terrain.Settings;

namespace PCG.Terrain.Performance.TestCase
{
    public sealed class TestInitInit : PerformanceCreator
    {
        public TestInitInit(ITerrainSettings terrainSettings) : base(terrainSettings)
        {
        }

        protected override void DefineSetUpSystems(IEcsSystemProxy system)
        {
            base.DefineSetUpSystems(system);
            system.Init<CreateSystem>(constructorArguments: TerrainSettings);
        }

        protected override void DefineRunSystems(IEcsSystemProxy system)
        {
            base.DefineRunSystems(system);
            system.Init<InitSystem>(false, constructorArguments: TerrainSettings);
        }
    }

    public sealed class TestInitRun : PerformanceCreator
    {
        public TestInitRun(ITerrainSettings terrainSettings) : base(terrainSettings)
        {
        }

        protected override void DefineSetUpSystems(IEcsSystemProxy system)
        {
            base.DefineSetUpSystems(system);
            system.Init<CreateSystem>(constructorArguments: TerrainSettings);
            system.Init<InitSystem>(false, constructorArguments: TerrainSettings);
        }

        protected override void DefineRunSystems(IEcsSystemProxy system)
        {
            base.DefineRunSystems(system);
            system.Get<InitSystem>();
        }
    }
}