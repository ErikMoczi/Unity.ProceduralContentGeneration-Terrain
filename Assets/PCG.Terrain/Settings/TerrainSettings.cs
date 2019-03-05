using System;
using PCG.Terrain.Common.Grid;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace PCG.Terrain.Settings
{
    [CreateAssetMenu(menuName = "TerrainConstruct/Terrain Settings", fileName = nameof(TerrainSettings))]
    public sealed class TerrainSettings :
#if UNITY_EDITOR
        ScriptableSingleton<TerrainSettings>
#else
        ScriptableObject
#endif
        , ITerrainSettings
    {
        public static readonly int GradientHeightTexture = Shader.PropertyToID("_gradientHeightTexture");
        public static readonly int ElevationMinMax = Shader.PropertyToID("_elevationMinMax");

        private const int MaxResolution = 255;
        private const int MaxChunks = 10000;

        [SerializeField, Range(1, MaxResolution)]
        private int resolution = 128;

#pragma warning disable 649
        [SerializeField, Range(1, MaxChunks)] private int chunkCount = 10;
        [SerializeField, Range(1, MaxChunks)] private int chunksPerFrame = 10;
        [SerializeField, Range(0, 10)] private int changeThreshold = 2;
        [SerializeField] private NoiseSettings noiseSettings;
        [SerializeField] private MeshSettings meshSettings;
        [SerializeField] private int gradientResolution = 256;
        [SerializeField] private Gradient gradient;
#pragma warning restore 649
        [NonSerialized] private int _totalVertices;
        [NonSerialized] private int _totalTriangles;
        [NonSerialized] private int _arrayChunk;

        public int Resolution => resolution;
        public int ChunkCount => chunkCount;
        public int ChunksPerFrame => chunksPerFrame;
        public int ChangeThreshold => changeThreshold;
        public NoiseSettings NoiseSettings => noiseSettings;
        public MeshSettings MeshSettings => meshSettings;
        public int TotalVertices => _totalVertices;
        public int TotalTriangles => _totalTriangles;
        public int ArrayChunk => _arrayChunk;
        public int GradientResolution => gradientResolution;
        public Gradient Gradient => gradient;

        private void Awake()
        {
            OnValidate();
        }

        public Texture2D GradientHeight()
        {
            var texture = new Texture2D(gradientResolution, 1, TextureFormat.RGBA32, false, true)
            {
                wrapMode = TextureWrapMode.Clamp
            };

            using (var gradientEvaluateJob = new GradientEvaluate(
                gradient,
                texture.GetRawTextureData<Color32>(),
                gradientResolution
            ))
            {
                gradientEvaluateJob.Schedule(gradientResolution, 1).Complete();
            }

            texture.Apply(false);

            return texture;
        }

        private void OnValidate()
        {
            chunksPerFrame = math.clamp(chunksPerFrame, 1, chunkCount);
            _totalVertices = (resolution + 1) * (resolution + 1);
            Assert.IsTrue(_totalVertices > 0);
            _totalTriangles = resolution * resolution * 6;
            Assert.IsTrue(_totalTriangles > 0);
            _arrayChunk = _totalVertices / Environment.TotalBufferEntities +
                          (_totalVertices % Environment.TotalBufferEntities > 0 ? 1 : 0);
        }
    }
}