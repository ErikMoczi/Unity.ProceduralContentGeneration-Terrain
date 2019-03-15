using PCG.Terrain.Settings;
using UnityEngine;

namespace PCG.Terrain.Performance.Utils
{
    public static class ResourcesData
    {
        private const string TestSettingsAsset = "TestSettings";
        private const string TerrainSettingsAsset = "TerrainSettings";

        public static ITerrainSettings LoadTerrainSettings()
        {
            return Resources.Load<TerrainSettings>(TerrainSettingsAsset);
        }

        public static ITestSettings LoadTestSettings()
        {
            return Resources.Load<TestSettings>(TestSettingsAsset);
        }
    }
}