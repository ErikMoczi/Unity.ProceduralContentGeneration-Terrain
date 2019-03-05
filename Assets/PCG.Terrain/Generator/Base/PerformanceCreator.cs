using PCG.Terrain.Settings;

namespace PCG.Terrain.Generator.Base
{
    public class PerformanceCreator : TerrainCreator
    {
        public PerformanceCreator(ITerrainSettings terrainSettings) : base(terrainSettings)
        {
        }

        protected sealed override void DefinePostSetUpSystems(IEcsSystemProxy system)
        {
            base.DefinePostSetUpSystems(system);
        }

        protected sealed override void DefinePostRunSystems(IEcsSystemProxy system)
        {
            base.DefinePostRunSystems(system);
        }
    }
}