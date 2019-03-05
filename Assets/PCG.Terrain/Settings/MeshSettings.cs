using System;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace PCG.Terrain.Settings
{
    [Serializable]
    public struct MeshSettings
    {
#pragma warning disable 649
        [SerializeField] private int subMesh;
        [SerializeField] private ShadowCastingMode castShadows;
        [SerializeField] private bool receiveShadows;
        [SerializeField] private Material material;
#pragma warning restore 649

        public int SubMesh => subMesh;
        public ShadowCastingMode CastShadows => castShadows;
        public bool ReceiveShadows => receiveShadows;
        public Material Material => material;

        public RenderMesh RenderMesh => new RenderMesh
        {
            material = material,
            subMesh = subMesh,
            castShadows = castShadows,
            receiveShadows = receiveShadows,
        };
    }
}