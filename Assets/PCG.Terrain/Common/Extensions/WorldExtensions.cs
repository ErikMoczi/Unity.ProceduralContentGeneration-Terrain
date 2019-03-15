using Unity.Entities;

namespace PCG.Terrain.Common.Extensions
{
    public static class WorldExtensions
    {
        public static void OneHopLifetime<T>(this World world, params object[] constructorArgumnents)
            where T : ComponentSystem
        {
            var system = world.CreateManager<T>(constructorArgumnents);
            system.RunOnce();
            world.DestroyManager(system);
        }
    }
}