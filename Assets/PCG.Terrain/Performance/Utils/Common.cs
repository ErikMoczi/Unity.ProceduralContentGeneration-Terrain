namespace PCG.Terrain.Performance.Utils
{
    public static class Common
    {
        public const string FirstKeyWord = "_First";
        public const string DefinitionPrefix = "#";

        public static string DefinitionName(string name, string suffix = "")
        {
            return $"{DefinitionPrefix}{name}{suffix}";
        }
    }
}