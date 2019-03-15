using System;
using UnityEngine;

namespace PCG.Terrain.Settings
{
    [Serializable]
    public struct NoiseSettings
    {
        #region DefaultRangeValues

        public const float MinFrequency = -10f;
        public const float MaxFrequency = 10f;
        public const int MinOctaves = 1;
        public const int MaxOctaves = 8;
        public const float MinLacunarity = 1f;
        public const float MaxLacunarity = 8f;
        public const float MinPersistence = 0f;
        public const float MaxPersistence = 1f;
        public const float MinAmplitude = 0f;
        public const float MaxAmplitude = 1f;

        #endregion

        #region SerializeField

#pragma warning disable 649
        [SerializeField, Range(MinFrequency, MaxFrequency)]
        private float frequency;

        [SerializeField, Range(MinOctaves, MaxOctaves)]
        private int octaves;

        [SerializeField, Range(MinLacunarity, MaxLacunarity)]
        private float lacunarity;

        [SerializeField, Range(MinPersistence, MaxPersistence)]
        private float persistence;

        [SerializeField, Range(MinAmplitude, MaxAmplitude)]
        private float amplitude;
#pragma warning restore 649

        #endregion

        public float Frequency => frequency;
        public int Octaves => octaves;
        public float Lacunarity => lacunarity;
        public float Persistence => persistence;
        public float Amplitude => amplitude;
    }

    public struct NoiseSettingsImpostor
    {
        public float frequency;
        public int octaves;
        public float lacunarity;
        public float persistence;
        public float amplitude;
    }
}