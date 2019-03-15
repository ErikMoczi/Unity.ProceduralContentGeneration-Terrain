using JetBrains.Annotations;
using PCG.Terrain.Settings;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace PCG.Terrain.Core
{
    public static class Bootstrap
    {
        private static TerrainSystemManager _terrainSystemManager;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeBeforeSceneLoad()
        {
            var world = new World("Terrain experiment");
            World.Active = world;

            _terrainSystemManager = new TerrainSystemManager(world);

            PlayerLoopManager.RegisterDomainUnload(DomainUnloadShutdown, 10000);
            world.CreateManager<EntityManager>();
            world.CreateManager<EndFrameBarrier>();
            world.CreateManager<RenderBoundsUpdateSystem>();
            world.CreateManager<RenderMeshSystemV2>();
            world.CreateManager<EndFrameTransformSystem>();
            world.CreateManager<CopyTransformFromGameObjectSystem>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeAfterSceneLoad()
        {
            var terrainController = Object.FindObjectOfType<TerrainController>();
            _terrainSystemManager.InitConfiguration(terrainController.TerrainSettings);
        }

        public static void LoadNewConfiguration([NotNull] ITerrainSettings terrainSettings)
        {
            _terrainSystemManager.InitConfiguration(terrainSettings);
        }

        private static void DomainUnloadShutdown()
        {
            _terrainSystemManager.Dispose();
            World.DisposeAllWorlds();

            WordStorage.Instance.Dispose();
            WordStorage.Instance = null;
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop();
        }
    }
}