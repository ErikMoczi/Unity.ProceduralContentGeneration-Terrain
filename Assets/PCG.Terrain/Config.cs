namespace PCG.Terrain
{
    public static class Environment
    {
        public const int ValuesPerEntity =
#if TERRAIN_BUFFER_SIZE_16
                16
#elif TERRAIN_BUFFER_SIZE_32
                32
#elif TERRAIN_BUFFER_SIZE_64
                64
#else
                128
#endif
            ;

        public const int BurstSimdSize = 4;
        public const int TotalBufferEntities = ValuesPerEntity * BurstSimdSize;
    }
}