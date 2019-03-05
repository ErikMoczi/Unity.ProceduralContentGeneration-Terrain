using PCG.Terrain.Settings;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;

namespace PCG.Terrain.Generator.Base
{
    public abstract class SimpleRunCreator : TerrainCreator
    {
        protected SimpleRunCreator(ITerrainSettings terrainSettings) : base(terrainSettings)
        {
        }

        protected sealed override void DefinePostSetUpSystems(IEcsSystemProxy system)
        {
            base.DefinePostSetUpSystems(system);
            system.Go<EndFrameTransformSystem>();
            system.Go<RenderMeshSystem>();
            system.Go<RenderBoundsUpdateSystem>();
        }

        protected sealed override void DefinePostRunSystems(IEcsSystemProxy system)
        {
            base.DefinePostRunSystems(system);
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(World.Active);
        }
    }
}