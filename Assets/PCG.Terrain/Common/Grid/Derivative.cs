using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace PCG.Terrain.Common.Grid
{
    public static class Derivative
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3x4 GetDerivative(in NativeArray<float3> vertices, int2x4 position, int resolution)
        {
            var derivativeX = -GetXDerivative(vertices, position, resolution);
            var derivativeZ = -GetZDerivative(vertices, position, resolution);
            return math.float3x4(
                math.normalize(math.float3(derivativeX.x, 1f, derivativeZ.x)),
                math.normalize(math.float3(derivativeX.y, 1f, derivativeZ.y)),
                math.normalize(math.float3(derivativeX.z, 1f, derivativeZ.z)),
                math.normalize(math.float3(derivativeX.w, 1f, derivativeZ.w))
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float4 GetXDerivative(in NativeArray<float3> vertices, int2x4 pos, int resolution)
        {
            var x = math.int4(pos.c0.x, pos.c1.x, pos.c2.x, pos.c3.x);
            var y = math.int4(pos.c0.y, pos.c1.y, pos.c2.y, pos.c3.y);

            var rowOffset = y * (resolution + 1);
            var index = x + rowOffset;
            var firstIndex = math.bool4(x > 0);
            var lastIndex = math.bool4(x < resolution);

            var p0 = math.int4(0);
            var p1 = math.int4(1);
            var p2 = math.int4(-1);

            var leftIndex = math.select(p0, p2, firstIndex);
            var rightIndex = math.select(p1, math.select(p0, p1, lastIndex), firstIndex);
            var scale = math.select(
                resolution,
                math.select(resolution, 0.5f * resolution, lastIndex),
                firstIndex
            );

            var right = math.float4(
                vertices[index.x + rightIndex.x].y,
                vertices[index.y + rightIndex.y].y,
                vertices[index.z + rightIndex.z].y,
                vertices[index.w + rightIndex.w].y
            );

            var left = math.float4(
                vertices[index.x + leftIndex.x].y,
                vertices[index.y + leftIndex.y].y,
                vertices[index.z + leftIndex.z].y,
                vertices[index.w + leftIndex.w].y
            );

            return (right - left) * scale;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float4 GetZDerivative(in NativeArray<float3> vertices, int2x4 pos, int resolution)
        {
            var x = math.int4(pos.c0.x, pos.c1.x, pos.c2.x, pos.c3.x);
            var y = math.int4(pos.c0.y, pos.c1.y, pos.c2.y, pos.c3.y);

            var rowLength = resolution + 1;
            var firstIndex = math.bool4(y > 0);
            var lastIndex = math.bool4(y < resolution);

            var p0 = math.int4(0);
            var p1 = math.int4(1);
            var p2 = math.int4(-1);

            var backIndex = math.select(p0, p2, firstIndex);
            var forwardIndex = math.select(p1, math.select(p0, p1, lastIndex), firstIndex);
            var scale = math.select(
                resolution,
                math.select(resolution, 0.5f * resolution, lastIndex),
                firstIndex
            );

            var forward = math.float4(
                vertices[(y.x + forwardIndex.x) * rowLength + x.x].y,
                vertices[(y.y + forwardIndex.y) * rowLength + x.y].y,
                vertices[(y.z + forwardIndex.z) * rowLength + x.z].y,
                vertices[(y.w + forwardIndex.w) * rowLength + x.w].y
            );

            var back = math.float4(
                vertices[(y.x + backIndex.x) * rowLength + x.x].y,
                vertices[(y.y + backIndex.y) * rowLength + x.y].y,
                vertices[(y.z + backIndex.z) * rowLength + x.z].y,
                vertices[(y.w + backIndex.w) * rowLength + x.w].y
            );

            return (forward - back) * scale;
        }

        private static float GetXDerivative(in NativeArray<float3> vertices, int resolution, int x, int z)
        {
            var rowOffset = z * (resolution + 1);
            float left, right, scale;
            if (x > 0)
            {
                left = vertices[rowOffset + x - 1].y;
                if (x < resolution)
                {
                    right = vertices[rowOffset + x + 1].y;
                    scale = 0.5f * resolution;
                }
                else
                {
                    right = vertices[rowOffset + x].y;
                    scale = resolution;
                }
            }
            else
            {
                left = vertices[rowOffset + x].y;
                right = vertices[rowOffset + x + 1].y;
                scale = resolution;
            }

            return (right - left) * scale;
        }

        private static float GetZDerivative(in NativeArray<float3> vertices, int resolution, int x, int z)
        {
            var rowLength = resolution + 1;
            float back, forward, scale;
            if (z > 0)
            {
                back = vertices[(z - 1) * rowLength + x].y;
                if (z < resolution)
                {
                    forward = vertices[(z + 1) * rowLength + x].y;
                    scale = 0.5f * resolution;
                }
                else
                {
                    forward = vertices[z * rowLength + x].y;
                    scale = resolution;
                }
            }
            else
            {
                back = vertices[z * rowLength + x].y;
                forward = vertices[(z + 1) * rowLength + x].y;
                scale = resolution;
            }

            return (forward - back) * scale;
        }
    }
}