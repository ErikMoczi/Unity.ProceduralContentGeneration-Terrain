using System;
using PCG.Terrain.Generator;
using PCG.Terrain.Generator.Base;
using PCG.Terrain.Settings;
using UnityEngine;

namespace PCG.Terrain
{
    public class TerrainProfiler : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private bool autoUpdate;
        [SerializeField] private TerrainCreatorType terrainCreatorType;
        [SerializeField] private TerrainSettings terrainSettings;
#pragma warning restore 649

        public bool AutoUpdate => autoUpdate;
        public bool Working { get; private set; }
        private TerrainCreator _currentCreator;

        public void SetUp()
        {
            Working = true;
            _currentCreator?.CleanUp();
            _currentCreator = GenerateNew();
            _currentCreator.SetUp();
            Working = false;
        }

        public void Run()
        {
            Working = true;
            _currentCreator?.Run();
            Working = false;
        }

        public void Clear()
        {
            Working = true;
            _currentCreator?.CleanUp();
            Working = false;
        }

        private TerrainCreator GenerateNew()
        {
            switch (terrainCreatorType)
            {
                case TerrainCreatorType.Batching:
                {
                    return new Batching(terrainSettings);
                }
                default:
                {
                    throw new Exception(
                        $"Missing implementation of {nameof(TerrainCreator)} for {terrainCreatorType}"
                    );
                }
            }
        }
    }
}