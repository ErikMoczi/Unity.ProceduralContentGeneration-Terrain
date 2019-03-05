using PCG.Terrain.Core.Components.Hybrid;
using PCG.Terrain.Core.Systems;
using PCG.Terrain.Generator.Base;
using PCG.Terrain.Settings;
using Unity.Transforms;
using UnityEngine;

namespace PCG.Terrain.Generator
{
    public sealed class Batching : SimpleRunCreator
    {
        public Batching(ITerrainSettings terrainSettings) : base(terrainSettings)
        {
        }

        protected override void DefineSetUpSystems(IEcsSystemProxy system)
        {
            base.DefineSetUpSystems(system);
            system.Init<CopyTransformFromGameObjectSystem>();
            system.Init<CreateSystem>(constructorArguments: TerrainSettings);
            system.Init<InitSystem>(constructorArguments: TerrainSettings);
            system.Init<TerrainSystem>(false, constructorArguments: TerrainSettings);
        }

        protected override void DefineRunSystems(IEcsSystemProxy system)
        {
            base.DefineRunSystems(system);
            CreatePlayer();
            system.Go<CopyTransformFromGameObjectSystem>();
            system.Go<TerrainSystem>();
        }

        private static void CreatePlayer()
        {
            var player = new GameObject("Player");
            player.AddComponent<PositionProxy>();
            player.AddComponent<CopyTransformFromGameObjectProxy>();
            player.AddComponent<FollowTargetProxy>();
        }
    }
}