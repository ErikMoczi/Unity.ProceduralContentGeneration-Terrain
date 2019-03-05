using System;
using UnityEngine;

namespace PCG.Terrain.Settings
{
    [Serializable]
    public struct NoiseSettings : INoiseSettings
    {
#pragma warning disable 649
        [SerializeField] private float frequency;
        [SerializeField, Range(1, 8)] private int octaves;
        [SerializeField, Range(1, 8)] private float lacunarity;
        [SerializeField, Range(0f, 1f)] private float persistence;
        [SerializeField, Range(0f, 1f)] private float amplitude;
#pragma warning restore 649

        public float Frequency => frequency;
        public int Octaves => octaves;
        public float Lacunarity => lacunarity;
        public float Persistence => persistence;
        public float Amplitude => amplitude;
    }
}