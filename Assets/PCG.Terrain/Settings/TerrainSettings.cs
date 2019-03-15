using System;
using NaughtyAttributes;
using PCG.Terrain.Common.Grid;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace PCG.Terrain.Settings
{
    [Serializable]
    [CreateAssetMenu(menuName = "TerrainConstruct/Terrain Settings", fileName = nameof(TerrainSettings))]
    public sealed unsafe class TerrainSettings :
#if UNITY_EDITOR
        ScriptableSingleton<TerrainSettings>
#else
        ScriptableObject
#endif
        , ITerrainSettingsImpostor
    {
        public static readonly int GradientHeightTexture = Shader.PropertyToID("_gradientHeightTexture");
        public static readonly int ElevationMinMax = Shader.PropertyToID("_elevationMinMax");

        #region DefaultRangeValues

        public const int MinChunkCount = 1;
        public const int MaxChunkCount = 1000;
        public const int MinChunksPerFrame = MinChunkCount;
        public const int MaxChunksPerFrame = MaxChunkCount;
        public const int MinThreshold = 0;
        public const int MaxThreshold = 5;

        public static readonly int[] PossibleResolutions =
        {
#if TERRAIN_BUFFER_SIZE_16
            7, 15, 23, 31, 39, 47, 55, 63, 71, 79, 87, 95, 103, 111, 119, 127, 135, 143, 151, 159, 167, 175, 183, 191,
            199, 207, 215, 223, 231, 239, 247, 255
#elif TERRAIN_BUFFER_SIZE_32
            15, 31, 47, 63, 79, 95, 111, 127, 143, 159, 175, 191, 207, 223, 239, 255
#elif TERRAIN_BUFFER_SIZE_64
            15, 31, 47, 63, 79, 95, 111, 127, 143, 159, 175, 191, 207, 223, 239, 255
#else
            31, 63, 95, 127, 159, 191, 223, 255
#endif
        };

        public static readonly int[] PossibleGradientResolutions =
        {
            64, 128, 256
        };

        #endregion

        #region SerializeField

#pragma warning disable 649
        [SerializeField, Dropdown(nameof(PossibleResolutions))]
        private int resolution = 127;

        [SerializeField, Range(MinChunkCount, MaxChunkCount)]
        private int chunkCount = 25;

        [SerializeField, Range(MinChunksPerFrame, MaxChunksPerFrame)]
        private int chunksPerFrame = 5;

        [SerializeField, Range(MinThreshold, MaxThreshold)]
        private int changeThreshold = 1;

        [SerializeField] private NoiseSettings noiseSettings;
        [SerializeField] private MeshSettings meshSettings;

        [SerializeField, Dropdown(nameof(PossibleGradientResolutions))]
        private int gradientResolution = 256;

        [SerializeField] private Gradient gradient;
#pragma warning restore 649

        #endregion

        [NonSerialized] private int _totalVertices;
        [NonSerialized] private int _totalTriangles;
        [NonSerialized] private int _arrayChunk;

        public int Resolution
        {
            get => resolution;
            set => resolution = value;
        }

        public int ChunkCount
        {
            get => chunkCount;
            set => chunkCount = value;
        }

        public int ChunksPerFrame
        {
            get => chunksPerFrame;
            set => chunksPerFrame = value;
        }

        public int ChangeThreshold
        {
            get => changeThreshold;
            set => changeThreshold = value;
        }

        public NoiseSettings NoiseSettings => noiseSettings;

        public ref NoiseSettingsImpostor NoiseSettingsImpostor
        {
            get
            {
                var ptr = UnsafeUtility.AddressOf(ref noiseSettings);
                return ref UnsafeUtilityEx.AsRef<NoiseSettingsImpostor>(ptr);
            }
        }

        public MeshSettings MeshSettings => meshSettings;
        public int TotalVertices => _totalVertices;
        public int TotalTriangles => _totalTriangles;
        public int ArrayChunk => _arrayChunk;

        private void Awake()
        {
            OnValidate();
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

        public object Clone()
        {
            var obj = MemberwiseClone();
            ((TerrainSettings) obj).OnValidate();
            return obj;
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
    }
}