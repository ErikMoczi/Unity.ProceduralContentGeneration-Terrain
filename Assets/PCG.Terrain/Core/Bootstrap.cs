using PCG.Terrain.Common.Extensions;
using PCG.Terrain.Core.Systems;
using PCG.Terrain.Settings;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace PCG.Terrain.Core
{
    public static class Bootstrap
    {
        private const string TerrainSettingsAsset = "TerrainSettings";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeBeforeSceneLoad()
        {
            var world = new World("Terrain experiment");
            World.Active = world;

            PlayerLoopManager.RegisterDomainUnload(DomainUnloadShutdown, 10000);
            world.CreateManager<EntityManager>();
            world.CreateManager<EndFrameBarrier>();
            world.CreateManager<RenderBoundsUpdateSystem>();
            world.CreateManager<RenderMeshSystemV2>();
            world.CreateManager<EndFrameTransformSystem>();
            world.CreateManager<CopyTransformFromGameObjectSystem>();

            var terrainSettings = LoadTerrainSettings();
            world.OneHopLifetime<CreateSystem>(terrainSettings);
            world.OneHopLifetime<InitSystem>(terrainSettings);
            world.CreateManager<TerrainSystem>(terrainSettings);
            
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(world);
        }

        private static void DomainUnloadShutdown()
        {
            World.DisposeAllWorlds();

            WordStorage.Instance.Dispose();
            WordStorage.Instance = null;
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop();
        }

        private static ITerrainSettings LoadTerrainSettings()
        {
            return Resources.Load<TerrainSettings>(TerrainSettingsAsset);
        }
    }
}