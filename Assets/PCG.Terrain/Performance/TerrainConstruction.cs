using NUnit.Framework;
using PCG.Terrain.Performance.TestCase;
using PCG.Terrain.Performance.TestCase.Base;
using PCG.Terrain.Performance.Utils;
using Unity.PerformanceTesting;

namespace PCG.Terrain.Performance
{
    [TestFixture(typeof(TestCreateInit))]
    [TestFixture(typeof(TestCreateRun))]
    [TestFixture(typeof(TestInitInit))]
    [TestFixture(typeof(TestInitRun))]
    [TestFixture(typeof(TestTerrainInit))]
    [TestFixture(typeof(TestTerrainRun))]
    [TestFixture(typeof(GeneralCreateSystem))]
    [TestFixture(typeof(FirstGenerateSystem))]
    [TestFixture(typeof(SecondGenerateSystem))]
    public sealed class TerrainConstruction<TTerrainCreator> : BaseTestTerrain<TTerrainCreator>
        where TTerrainCreator : class, ITerrainCreator
    {
        protected override void MainWork(bool firstRun = false, bool measure = true)
        {
            TerrainCreator.SetUp();
            if (measure)
            {
                using (Measure.Scope(new SampleGroupDefinition(
                    Utils.Common.DefinitionName(
                        TerrainCreator.GetType().Name,
                        firstRun ? Utils.Common.FirstKeyWord : string.Empty
                    ),
                    TestSettings.SampleUnit
                )))
                {
                    RunStatement();
                }
            }
            else
            {
                RunStatement();
            }

            TerrainCreator.CleanUp();
        }

        private void RunStatement()
        {
            TerrainCreator.Run();
        }
    }
}