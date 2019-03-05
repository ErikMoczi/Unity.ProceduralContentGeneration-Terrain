using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace PCG.Terrain.Common.Grid
{
    public static class MeshCreator
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GridData(ref NativeArray<float3> vertices, ref NativeArray<int> triangles, int index,
            int resolution, float stepSize)
        {
            var position = Location.Position(index, resolution);
            vertices[index] = Vertex(stepSize, position);
            Triangle(ref triangles, index, resolution, position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float3 Vertex(float stepSize, int2 position)
        {
            return math.float3(position.x * stepSize - 0.5f, 1f, position.y * stepSize - 0.5f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Triangle(ref NativeArray<int> triangles, int index, int resolution, int2 position)
        {
            if (position.x < resolution && position.y < resolution)
            {
                var t = 6 * (index - position.y);
                triangles[t] = index;
                triangles[t + 1] = index + resolution + 1;
                triangles[t + 2] = index + 1;
                triangles[t + 3] = index + 1;
                triangles[t + 4] = index + resolution + 1;
                triangles[t + 5] = index + resolution + 2;
            }
        }
    }
}