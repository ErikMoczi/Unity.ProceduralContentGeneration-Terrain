namespace PCG.Terrain
{
    public static class Environment
    {
        public const int ValuesPerEntity = 128;
        public const int BurstSimdSize = 4;
        public const int TotalBufferEntities = ValuesPerEntity * BurstSimdSize;
    }
}