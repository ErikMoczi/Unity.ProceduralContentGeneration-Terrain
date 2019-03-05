using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace PCG.Terrain.Common.Grid
{
    public static class Location
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 NextIndexes(int index)
        {
            return math.int4(
                index,
                index + 1,
                index + 2,
                index + 3
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 Index2DFrom1D(int index, int size)
        {
            return math.int2(index % size, index / size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2x4 Index2DFrom1D(int4 index, int size)
        {
            return math.int2x4(
                Index2DFrom1D(index.x, size),
                Index2DFrom1D(index.y, size),
                Index2DFrom1D(index.z, size),
                Index2DFrom1D(index.w, size)
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Index1DFrom2D(int2 position, int size)
        {
            return position.x + position.y * size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 Index1DFrom2D(int2x4 position, int size)
        {
            return math.int4(
                Index1DFrom2D(position.c0, size),
                Index1DFrom2D(position.c1, size),
                Index1DFrom2D(position.c2, size),
                Index1DFrom2D(position.c3, size)
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2x4 Position(in int4 index, in int resolution)
        {
            return math.int2x4(
                Position(index.x, resolution),
                Position(index.y, resolution),
                Position(index.z, resolution),
                Position(index.w, resolution)
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 Position(in int index, in int resolution)
        {
            var size = resolution + 1;
            return Index2DFrom1D(index, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 SpiralChunkPosition(int n)
        {
            var m = (int) math.floor((math.sqrt(n) + 1) / 2);
            var k = n - 4 * m * (m - 1);
            var pos = math.int2(0f);
            if (k <= 2 * m)
            {
                pos.x = m;
                pos.y = k - m;
            }
            else if (k <= 4 * m)
            {
                pos.x = 3 * m - k;
                pos.y = m;
            }
            else if (k <= 6 * m)
            {
                pos.x = -m;
                pos.y = 5 * m - k;
            }
            else
            {
                pos.x = k - 7 * m;
                pos.y = -m;
            }

            return pos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SpiralChunkIndex(int2 position)
        {
            var m = math.max(math.abs(position.x), math.abs(position.y));
            if (position.x == m && position.y != -m)
            {
                return 4 * m * (m - 1) + m + position.y;
            }

            if (position.y == m)
            {
                return 4 * m * (m - 1) + 3 * m - position.x;
            }

            if (position.x == -m)
            {
                return 4 * m * (m - 1) + 5 * m - position.y;
            }

            return 4 * m * (m - 1) + 7 * m + position.x;
        }
    }
}