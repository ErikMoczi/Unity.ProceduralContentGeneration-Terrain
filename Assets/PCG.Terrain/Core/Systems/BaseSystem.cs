using PCG.Terrain.Settings;
using Unity.Entities;

namespace PCG.Terrain.Core.Systems
{
    [DisableAutoCreation]
    public abstract class BaseSystem : ComponentSystem
    {
        protected ITerrainSettings TerrainSettings { get; }

        public BaseSystem(ITerrainSettings terrainSettings)
        {
            TerrainSettings = terrainSettings;
        }
    }
}