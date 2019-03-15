using System;
using NUnit.Framework;
using PCG.Terrain.Performance.TestCase.Base;
using PCG.Terrain.Settings;
using Unity.PerformanceTesting;

namespace PCG.Terrain.Performance.Utils
{
    [Category("Performance")]
    public abstract class BaseTestTerrain<TTerrainCreator>
        where TTerrainCreator : class, ITerrainCreator
    {
        protected ITestSettings TestSettings { get; private set; }
        protected ITerrainSettings TerrainSettings { get; private set; }
        protected TTerrainCreator TerrainCreator { get; private set; }

        #region Runner

        [PerformanceTest]
        public void TerrainConstruction_Test()
        {
            #region FirstRun

            MainWork(true);

            #endregion

            #region WarmUp

            for (var i = 0; i < TestSettings.WarmUpCount; i++)
            {
                MainWork(measure: false);
            }

            #endregion

            #region TestCase

            for (var i = 0; i < TestSettings.TotalRuns; i++)
            {
                MainWork();
            }

            #endregion
        }

        #endregion

        #region SetUp

        [SetUp]
        public void SetUp()
        {
            TestSettings = ResourcesData.LoadTestSettings();
            TerrainSettings = ResourcesData.LoadTerrainSettings();
            TerrainCreator = InitTerrainCreator();
        }

        #endregion

        #region Helpers

        protected abstract void MainWork(bool firstRun = false, bool measure = true);

        private TTerrainCreator InitTerrainCreator()
        {
            return (TTerrainCreator) Activator.CreateInstance(typeof(TTerrainCreator), TerrainSettings);
        }

        #endregion
    }
}