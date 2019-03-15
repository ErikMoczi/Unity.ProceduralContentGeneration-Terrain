using System.Collections.Generic;
using PCG.Terrain.Settings;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;

namespace PCG.Terrain.Performance.TestCase.Base
{
    public abstract class TerrainCreator : ITerrainCreator
    {
        protected interface IEcsSystemProxy
        {
            T Init<T>(bool runUpdate = true, params object[] constructorArguments) where T : ComponentSystemBase;
            T Get<T>(bool runUpdate = true) where T : ComponentSystemBase;
        }

        #region EcsSystemProxy

        private interface IInternalEcsSystemProxy : IEcsSystemProxy
        {
            void Clear();
            void Run();
        }

        private sealed class EcsSystemProxy : IInternalEcsSystemProxy
        {
            private readonly List<ComponentSystemBase> _systems = new List<ComponentSystemBase>();

            public T Init<T>(bool runUpdate = true, params object[] constructorArguments)
                where T : ComponentSystemBase
            {
                var system = World.Active.CreateManager<T>(constructorArguments);
                system.Enabled = false;
                if (runUpdate)
                {
                    _systems.Add(system);
                }

                return system;
            }

            public T Get<T>(bool runUpdate = true)
                where T : ComponentSystemBase
            {
                var system = World.Active.GetExistingManager<T>();
                system.Enabled = false;
                if (runUpdate)
                {
                    _systems.Add(system);
                }

                return system;
            }

            public void Run()
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < _systems.Count; i++)
                {
                    _systems[i].Enabled = true;
                    _systems[i].Update();
                    _systems[i].Enabled = false;
                }
            }

            public void Clear()
            {
                _systems.Clear();
            }
        }

        #endregion

        protected ITerrainSettings TerrainSettings { get; }
        private const string WorldName = "TestWorld";
        private readonly IInternalEcsSystemProxy _system = new EcsSystemProxy();

        // ReSharper disable once PublicConstructorInAbstractClass
        public TerrainCreator(ITerrainSettings terrainSettings)
        {
            TerrainSettings = terrainSettings;
        }

        public void SetUp()
        {
            World.DisposeAllWorlds();
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(World.Active);
            CreateDefaultWorld();

            _system.Init<RenderMeshSystemV2>();
            _system.Init<RenderBoundsUpdateSystem>();
            _system.Init<EndFrameTransformSystem>();

            DefineSetUpSystems(_system);

            _system.Get<EndFrameTransformSystem>();
            _system.Get<RenderBoundsUpdateSystem>();
            _system.Get<RenderMeshSystemV2>();

            DefinePostSetUpSystems(_system);

            _system.Run();
            _system.Clear();

            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(World.Active);
        }

        public void Run()
        {
            DefineRunSystems(_system);
            DefinePostRunSystems(_system);
            _system.Run();
        }

        public void CleanUp()
        {
            _system.Clear();
            World.DisposeAllWorlds();
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(World.Active);
        }

        protected virtual void DefinePostSetUpSystems(IEcsSystemProxy system)
        {
        }

        protected virtual void DefinePostRunSystems(IEcsSystemProxy system)
        {
        }

        protected virtual void DefineSetUpSystems(IEcsSystemProxy system)
        {
        }

        protected virtual void DefineRunSystems(IEcsSystemProxy system)
        {
        }

        private static void CreateDefaultWorld()
        {
            var world = new World(WorldName);
            World.Active = world;
            world.CreateManager<EntityManager>();
        }
    }
}