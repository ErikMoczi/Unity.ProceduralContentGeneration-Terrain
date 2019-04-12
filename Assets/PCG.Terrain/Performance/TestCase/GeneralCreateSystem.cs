using PCG.Terrain.Core.Components.Hybrid;
using PCG.Terrain.Core.Systems;
using PCG.Terrain.Performance.TestCase.Base;
using PCG.Terrain.Settings;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace PCG.Terrain.Performance.TestCase
{
    public sealed class GeneralCreateSystem : PerformanceCreator
    {
        public GeneralCreateSystem(ITerrainSettings terrainSettings) : base(terrainSettings)
        {
        }

        protected override void DefineRunSystems(IEcsSystemProxy system)
        {
            base.DefineRunSystems(system);
            system.Init<CreateSystem>(constructorArguments: TerrainSettings);
            system.Init<InitSystem>(constructorArguments: TerrainSettings);
            system.Init<TerrainSystem>(false, constructorArguments: TerrainSettings);
        }
    }

    public sealed class FirstGenerateSystem : PerformanceCreator
    {
        public FirstGenerateSystem(ITerrainSettings terrainSettings) : base(terrainSettings)
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

    public sealed class SecondGenerateSystem : PerformanceCreator
    {
        private const string TargetFollow = "Player";
        private Transform _playerPosition;

        public SecondGenerateSystem(ITerrainSettings terrainSettings) : base(terrainSettings)
        {
        }

        protected override void DefineSetUpSystems(IEcsSystemProxy system)
        {
            base.DefineSetUpSystems(system);
            CreatePlayer();
            system.Init<CreateSystem>(constructorArguments: TerrainSettings);
            system.Init<InitSystem>(constructorArguments: TerrainSettings);
            system.Init<TerrainSystem>(constructorArguments: TerrainSettings);
        }

        protected override void DefinePostSetUpSystems(IEcsSystemProxy system)
        {
            base.DefinePostSetUpSystems(system);
            system.Get<TerrainSystem>();
            system.Get<TerrainSystem>();
            system.Get<TerrainSystem>();
            system.Get<CopyTransformFromGameObjectSystem>();
            system.Get<EndFrameTransformSystem>();
            system.Get<EndFrameBarrier>();
        }

        protected override void DefineRunSystems(IEcsSystemProxy system)
        {
            base.DefineRunSystems(system);
            _playerPosition.position = new Vector3(100f, 0f, 100f);
            system.Get<CopyTransformFromGameObjectSystem>();
            system.Get<EndFrameTransformSystem>();
            system.Get<EndFrameBarrier>();
            system.Get<TerrainSystem>();
        }

        protected override void AfterCleanup()
        {
            base.AfterCleanup();
            Object.Destroy(GameObject.Find(TargetFollow));
        }

        private void CreatePlayer()
        {
            var obj = new GameObject(TargetFollow);
            _playerPosition = obj.transform;
            obj.AddComponent<GameObjectEntity>();
            obj.AddComponent<FollowTargetProxy>();
            obj.AddComponent<PositionProxy>();
            obj.AddComponent<CopyInitialTransformFromGameObjectProxy>();
            obj.AddComponent<CopyTransformFromGameObjectProxy>();
        }
    }
}