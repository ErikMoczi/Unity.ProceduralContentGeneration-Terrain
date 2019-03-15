using PCG.Terrain.Core.Components;
using PCG.Terrain.Settings;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace PCG.Terrain.Core.Systems
{
    [DisableAutoCreation]
    public sealed class CleanUpSystem : ComponentSystem
    {
        private ComponentGroup _verticesGroup;
        private ComponentGroup _noiseGroup;
        private ComponentGroup _meshGroup;

        protected override void OnCreateManager()
        {
            base.OnCreateManager();
            _verticesGroup = GetComponentGroup(typeof(VerticesData));
            _noiseGroup = GetComponentGroup(typeof(NoiseCalculation));
            _meshGroup = GetComponentGroup(typeof(RenderMesh));
        }

        protected override void OnUpdate()
        {
            ForEach((RenderMesh renderMesh) =>
            {
                Object.Destroy(renderMesh.mesh);
                var texture = renderMesh.material.GetTexture(TerrainSettings.GradientHeightTexture);
                if (texture != null)
                {
                    Object.Destroy(texture);
                }
            });
            EntityManager.DestroyEntity(_verticesGroup);
            EntityManager.DestroyEntity(_noiseGroup);
            EntityManager.DestroyEntity(_meshGroup);
        }
    }
}