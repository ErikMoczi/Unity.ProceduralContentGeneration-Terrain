using PCG.Terrain.Common.Grid;
using PCG.Terrain.Settings;
using Unity.Mathematics;

namespace PCG.Terrain.Utilities
{
    public static class Noise
    {
        public static float4 CalculateNoise(int4 indexes, int2 offset, MeshAbout meshAbout)
        {
            var position = Location.Position(indexes, meshAbout.Size);
            var middle = MiddlePosition(position, meshAbout.StepSize, offset);
            return NoiseValue(middle, meshAbout.NoiseSettings);
        }

        private static float2x4 MiddlePosition(int2x4 position, float stepSize, int2 offset)
        {
            var p00 = math.float2(-0.5f, -0.5f) + offset;
            var p10 = math.float2(0.5f, -0.5f) + offset;
            var p01 = math.float2(-0.5f, 0.5f) + offset;
            var p11 = math.float2(0.5f, 0.5f) + offset;

            var point00 = math.float2x4(p00, p00, p00, p00);
            var point10 = math.float2x4(p10, p10, p10, p10);
            var point01 = math.float2x4(p01, p01, p01, p01);
            var point11 = math.float2x4(p11, p11, p11, p11);

            var point0 = math.float2x4(
                math.lerp(point00.c0, point01.c0, position.c0.y * stepSize),
                math.lerp(point00.c1, point01.c1, position.c1.y * stepSize),
                math.lerp(point00.c2, point01.c2, position.c2.y * stepSize),
                math.lerp(point00.c3, point01.c3, position.c3.y * stepSize)
            );

            var point1 = math.float2x4(
                math.lerp(point10.c0, point11.c0, position.c0.y * stepSize),
                math.lerp(point10.c1, point11.c1, position.c1.y * stepSize),
                math.lerp(point10.c2, point11.c2, position.c2.y * stepSize),
                math.lerp(point10.c3, point11.c3, position.c3.y * stepSize)
            );

            return math.float2x4(
                math.lerp(point0.c0, point1.c0, position.c0.x * stepSize),
                math.lerp(point0.c1, point1.c1, position.c1.x * stepSize),
                math.lerp(point0.c2, point1.c2, position.c2.x * stepSize),
                math.lerp(point0.c3, point1.c3, position.c3.x * stepSize)
            );
        }

        private static float4 NoiseValue(float2x4 point, NoiseSettings config)
        {
            var frequency = math.float4(config.Frequency);
            var amplitude = math.float4(config.Amplitude);
            var range = 1f * amplitude;
            var value = GetNoise(point, frequency, amplitude);
            for (var o = 1; o < config.Octaves; o++)
            {
                frequency *= config.Lacunarity;
                amplitude *= config.Persistence;
                range += amplitude;
                value += GetNoise(point, frequency, amplitude);
            }

            return value * (1f / range);
        }

        private static float4 GetNoise(float2x4 point, float4 frequency, float4 amplitude)
        {
            return cnoise(math.float2x4(
                       math.float2(point.c0.x * frequency.x, point.c0.y * frequency.x),
                       math.float2(point.c1.x * frequency.y, point.c1.y * frequency.y),
                       math.float2(point.c2.x * frequency.z, point.c2.y * frequency.z),
                       math.float2(point.c3.x * frequency.w, point.c3.y * frequency.w)
                   )) * amplitude;
        }

        #region cnoise

        static float mod289(float x)
        {
            return x - math.floor(x * (1.0f / 289.0f)) * 289.0f;
        }

        static float2 mod289(float2 x)
        {
            return x - math.floor(x * (1.0f / 289.0f)) * 289.0f;
        }

        static float3 mod289(float3 x)
        {
            return x - math.floor(x * (1.0f / 289.0f)) * 289.0f;
        }

        static float4 mod289(float4 x)
        {
            return x - math.floor(x * (1.0f / 289.0f)) * 289.0f;
        }

        static float4x4 mod289(float4x4 x)
        {
            return math.float4x4(
                x.c0 - math.floor(x.c0 * (1.0f / 289.0f)) * 289.0f,
                x.c1 - math.floor(x.c1 * (1.0f / 289.0f)) * 289.0f,
                x.c2 - math.floor(x.c2 * (1.0f / 289.0f)) * 289.0f,
                x.c3 - math.floor(x.c3 * (1.0f / 289.0f)) * 289.0f
            );
        }

        static float permute(float x)
        {
            return mod289((34.0f * x + 1.0f) * x);
        }

        static float3 permute(float3 x)
        {
            return mod289((34.0f * x + 1.0f) * x);
        }

        static float4 permute(float4 x)
        {
            return mod289((34.0f * x + 1.0f) * x);
        }

        static float4x4 permute(float4x4 x)
        {
            return mod289((34.0f * x + 1.0f) * x);
        }

        static float2 fade(float2 t)
        {
            return t * t * t * (t * (t * 6.0f - 15.0f) + 10.0f);
        }

        static float3 fade(float3 t)
        {
            return t * t * t * (t * (t * 6.0f - 15.0f) + 10.0f);
        }

        static float4 fade(float4 t)
        {
            return t * t * t * (t * (t * 6.0f - 15.0f) + 10.0f);
        }

        public static float4 frac(float4 x)
        {
            return x - math.floor(x);
        }

        public static float4x4 frac(float4x4 x)
        {
            return math.float4x4(
                x.c0 - math.floor(x.c0),
                x.c1 - math.floor(x.c1),
                x.c2 - math.floor(x.c2),
                x.c3 - math.floor(x.c3)
            );
        }

        static float4 taylorInvSqrt(float4 r)
        {
            return 1.79284291400159f - 0.85373472095314f * r;
        }

        static float4x4 taylorInvSqrt(float4x4 r)
        {
            return 1.79284291400159f - 0.85373472095314f * r;
        }

        public static float4 cnoise(float2x4 P)
        {
            float4x4 Pi = math.float4x4(
                math.floor(P.c0.xyxy) + math.float4(0.0f, 0.0f, 1.0f, 1.0f),
                math.floor(P.c1.xyxy) + math.float4(0.0f, 0.0f, 1.0f, 1.0f),
                math.floor(P.c2.xyxy) + math.float4(0.0f, 0.0f, 1.0f, 1.0f),
                math.floor(P.c3.xyxy) + math.float4(0.0f, 0.0f, 1.0f, 1.0f)
            );
            float4x4 Pf = math.float4x4(
                frac(P.c0.xyxy) - math.float4(0.0f, 0.0f, 1.0f, 1.0f),
                frac(P.c1.xyxy) - math.float4(0.0f, 0.0f, 1.0f, 1.0f),
                frac(P.c2.xyxy) - math.float4(0.0f, 0.0f, 1.0f, 1.0f),
                frac(P.c3.xyxy) - math.float4(0.0f, 0.0f, 1.0f, 1.0f)
            );
            Pi = mod289(Pi);


            float4x4 ix = math.float4x4(
                Pi.c0.xzxz,
                Pi.c1.xzxz,
                Pi.c2.xzxz,
                Pi.c3.xzxz
            );
            float4x4 iy = math.float4x4(
                Pi.c0.yyww,
                Pi.c1.yyww,
                Pi.c2.yyww,
                Pi.c3.yyww
            );
            float4x4 fx = math.float4x4(
                Pf.c0.xzxz,
                Pf.c1.xzxz,
                Pf.c2.xzxz,
                Pf.c3.xzxz
            );
            float4x4 fy = math.float4x4(
                Pf.c0.yyww,
                Pf.c1.yyww,
                Pf.c2.yyww,
                Pf.c3.yyww
            );

            float4x4 i = permute(permute(ix) + iy);

            float4x4 gx = frac(i * (1.0f / 41.0f)) * 2.0f - 1.0f;
            float4x4 gy = math.float4x4(
                math.abs(gx.c0) - 0.5f,
                math.abs(gx.c1) - 0.5f,
                math.abs(gx.c2) - 0.5f,
                math.abs(gx.c3) - 0.5f
            );
            float4x4 tx = math.float4x4(
                math.floor(gx.c0 + 0.5f),
                math.floor(gx.c1 + 0.5f),
                math.floor(gx.c2 + 0.5f),
                math.floor(gx.c3 + 0.5f)
            );
            gx = gx - tx;

            float2x4 g00 = math.float2x4(
                math.float2(gx.c0.x, gy.c0.x),
                math.float2(gx.c1.x, gy.c1.x),
                math.float2(gx.c2.x, gy.c2.x),
                math.float2(gx.c3.x, gy.c3.x)
            );
            float2x4 g10 = math.float2x4(
                math.float2(gx.c0.y, gy.c0.y),
                math.float2(gx.c1.y, gy.c1.y),
                math.float2(gx.c2.y, gy.c2.y),
                math.float2(gx.c3.y, gy.c3.y)
            );
            float2x4 g01 = math.float2x4(
                math.float2(gx.c0.z, gy.c0.z),
                math.float2(gx.c1.z, gy.c1.z),
                math.float2(gx.c2.z, gy.c2.z),
                math.float2(gx.c3.z, gy.c3.z)
            );
            float2x4 g11 = math.float2x4(
                math.float2(gx.c0.w, gy.c0.w),
                math.float2(gx.c1.w, gy.c1.w),
                math.float2(gx.c2.w, gy.c2.w),
                math.float2(gx.c3.w, gy.c3.w)
            );

            float4x4 norm = taylorInvSqrt(math.float4x4(
                math.float4(math.dot(g00.c0, g00.c0), math.dot(g01.c0, g01.c0), math.dot(g10.c0, g10.c0),
                    math.dot(g11.c0, g11.c0)),
                math.float4(math.dot(g00.c1, g00.c1), math.dot(g01.c1, g01.c1), math.dot(g10.c1, g10.c1),
                    math.dot(g11.c1, g11.c1)),
                math.float4(math.dot(g00.c2, g00.c2), math.dot(g01.c2, g01.c2), math.dot(g10.c2, g10.c2),
                    math.dot(g11.c2, g11.c2)),
                math.float4(math.dot(g00.c3, g00.c3), math.dot(g01.c3, g01.c3), math.dot(g10.c3, g10.c3),
                    math.dot(g11.c3, g11.c3))
            ));

            g00 = math.float2x4(
                math.float2(g00.c0.x * norm.c0.x, g00.c0.y * norm.c0.x),
                math.float2(g00.c1.x * norm.c1.x, g00.c1.y * norm.c1.x),
                math.float2(g00.c2.x * norm.c2.x, g00.c2.y * norm.c2.x),
                math.float2(g00.c3.x * norm.c3.x, g00.c3.y * norm.c3.x)
            );
            g01 = math.float2x4(
                math.float2(g01.c0.x * norm.c0.y, g01.c0.y * norm.c0.y),
                math.float2(g01.c1.x * norm.c1.y, g01.c1.y * norm.c1.y),
                math.float2(g01.c2.x * norm.c2.y, g01.c2.y * norm.c2.y),
                math.float2(g01.c3.x * norm.c3.y, g01.c3.y * norm.c3.y)
            );
            g10 = math.float2x4(
                math.float2(g10.c0.x * norm.c0.z, g10.c0.y * norm.c0.z),
                math.float2(g10.c1.x * norm.c1.z, g10.c1.y * norm.c1.z),
                math.float2(g10.c2.x * norm.c2.z, g10.c2.y * norm.c2.z),
                math.float2(g10.c3.x * norm.c3.z, g10.c3.y * norm.c3.z)
            );
            g11 = math.float2x4(
                math.float2(g11.c0.x * norm.c0.w, g11.c0.y * norm.c0.w),
                math.float2(g11.c1.x * norm.c1.w, g11.c1.y * norm.c1.w),
                math.float2(g11.c2.x * norm.c2.w, g11.c2.y * norm.c2.w),
                math.float2(g11.c3.x * norm.c3.w, g11.c3.y * norm.c3.w)
            );

            float4 n00 = math.float4(
                math.dot(g00.c0, math.float2(fx.c0.x, fy.c0.x)),
                math.dot(g00.c1, math.float2(fx.c1.x, fy.c1.x)),
                math.dot(g00.c2, math.float2(fx.c2.x, fy.c2.x)),
                math.dot(g00.c3, math.float2(fx.c3.x, fy.c3.x))
            );
            float4 n10 = math.float4(
                math.dot(g10.c0, math.float2(fx.c0.y, fy.c0.y)),
                math.dot(g10.c1, math.float2(fx.c1.y, fy.c1.y)),
                math.dot(g10.c2, math.float2(fx.c2.y, fy.c2.y)),
                math.dot(g10.c3, math.float2(fx.c3.y, fy.c3.y))
            );
            float4 n01 = math.float4(
                math.dot(g01.c0, math.float2(fx.c0.z, fy.c0.z)),
                math.dot(g01.c1, math.float2(fx.c1.z, fy.c1.z)),
                math.dot(g01.c2, math.float2(fx.c2.z, fy.c2.z)),
                math.dot(g01.c3, math.float2(fx.c3.z, fy.c3.z))
            );
            float4 n11 = math.float4(
                math.dot(g11.c0, math.float2(fx.c0.w, fy.c0.w)),
                math.dot(g11.c1, math.float2(fx.c1.w, fy.c1.w)),
                math.dot(g11.c2, math.float2(fx.c2.w, fy.c2.w)),
                math.dot(g11.c3, math.float2(fx.c3.w, fy.c3.w))
            );
            float2x4 fade_xy = math.float2x4(
                fade(Pf.c0.xy),
                fade(Pf.c1.xy),
                fade(Pf.c2.xy),
                fade(Pf.c3.xy)
            );
            float2x4 n_x = math.float2x4(
                math.lerp(math.float2(n00.x, n01.x), math.float2(n10.x, n11.x), fade_xy.c0.x),
                math.lerp(math.float2(n00.y, n01.y), math.float2(n10.y, n11.y), fade_xy.c1.x),
                math.lerp(math.float2(n00.z, n01.z), math.float2(n10.z, n11.z), fade_xy.c2.x),
                math.lerp(math.float2(n00.w, n01.w), math.float2(n10.w, n11.w), fade_xy.c3.x)
            );
            float4 n_xy = math.float4(
                math.lerp(n_x.c0.x, n_x.c0.y, fade_xy.c0.y),
                math.lerp(n_x.c1.x, n_x.c1.y, fade_xy.c1.y),
                math.lerp(n_x.c2.x, n_x.c2.y, fade_xy.c2.y),
                math.lerp(n_x.c3.x, n_x.c3.y, fade_xy.c3.y)
            );

            return 2.3f * n_xy;
        }

        #endregion
    }
}