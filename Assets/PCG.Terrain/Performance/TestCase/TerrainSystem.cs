using PCG.Terrain.Core.Systems;
using PCG.Terrain.Performance.TestCase.Base;
using PCG.Terrain.Settings;

namespace PCG.Terrain.Performance.TestCase
{
    public sealed class TestTerrainInit : PerformanceCreator
    {
        public TestTerrainInit(ITerrainSettings terrainSettings) : base(terrainSettings)
        {
        }

        protected override void DefineSetUpSystems(IEcsSystemProxy system)
        {
            base.DefineSetUpSystems(system);
            system.Init<CreateSystem>(constructorArguments: TerrainSettings);
            system.Init<InitSystem>(constructorArguments: TerrainSettings);
        }

        protected override void DefineRunSystems(IEcsSystemProxy system)
        {
            base.DefineRunSystems(system);
            system.Init<TerrainSystem>(false, constructorArguments: TerrainSettings);
        }
    }

    public sealed class TestTerrainRun : PerformanceCreator
    {
        public TestTerrainRun(ITerrainSettings terrainSettings) : base(terrainSettings)
        {
        }

        protected override void DefineSetUpSystems(IEcsSystemProxy system)
        {
            base.DefineSetUpSystems(system);
            system.Init<CreateSystem>(constructorArguments: TerrainSettings);
            system.Init<InitSystem>(constructorArguments: TerrainSettings);
            system.Init<TerrainSystem>(false, constructorArguments: TerrainSettings);
        }

        protected override void DefineRunSystems(IEcsSystemProxy system)
        {
            base.DefineRunSystems(system);
            system.Get<TerrainSystem>();
        }
    }
}