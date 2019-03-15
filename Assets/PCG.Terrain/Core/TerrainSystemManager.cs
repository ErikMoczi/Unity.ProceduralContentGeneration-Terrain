using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using PCG.Terrain.Common.Extensions;
using PCG.Terrain.Core.Systems;
using PCG.Terrain.Settings;
using Unity.Entities;

namespace PCG.Terrain.Core
{
    public sealed class TerrainSystemManager : IDisposable
    {
        private World _world;
        private List<ComponentSystemBase> _systems = new List<ComponentSystemBase>();

        public TerrainSystemManager(World world)
        {
            _world = world;
        }

        public void InitConfiguration([NotNull] ITerrainSettings terrainSettings)
        {
            CleanUp();
            SetUp(terrainSettings.Clone() as ITerrainSettings);
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(_world);
        }

        private void SetUp(ITerrainSettings terrainSettings)
        {
            _world.OneHopLifetime<CreateSystem>(terrainSettings);
            _world.OneHopLifetime<InitSystem>(terrainSettings);
            _systems.Add(_world.CreateManager<TerrainSystem>(terrainSettings));
        }

        private void CleanUp()
        {
            foreach (var system in _systems)
            {
                system.Enabled = false;
                _world.DestroyManager(system);
            }

            _systems.Clear();
            _world.OneHopLifetime<CleanUpSystem>();
        }

        public void Dispose()
        {
            _world = null;
            _systems.Clear();
            _systems = null;
        }
    }
}