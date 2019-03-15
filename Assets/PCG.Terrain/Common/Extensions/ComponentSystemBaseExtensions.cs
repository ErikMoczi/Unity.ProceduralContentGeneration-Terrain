using Unity.Entities;

namespace PCG.Terrain.Common.Extensions
{
    public static class ComponentSystemBaseExtensions
    {
        public static void RunOnce(this ComponentSystemBase system)
        {
            system.Enabled = true;
            system.Update();
            system.Enabled = false;
        }
    }
}