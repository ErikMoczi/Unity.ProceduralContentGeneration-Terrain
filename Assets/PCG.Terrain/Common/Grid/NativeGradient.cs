using System;
using System.Runtime.InteropServices;
using PCG.Terrain.Common.Collections.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace PCG.Terrain.Common.Grid
{
    [BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
    public struct GradientEvaluate : IJobParallelFor, IDisposable
    {
        #region DataStructures

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeGradient : IDisposable
        {
            private readonly GradientMode _gradientMode;
            private UnsafeArrayList<GradientColorKey> _gradientColorKeys;

            public unsafe NativeGradient(Gradient gradient, Allocator allocator)
            {
                _gradientMode = gradient.mode;
                var gradientColorKeys = gradient.colorKeys;
                _gradientColorKeys = new UnsafeArrayList<GradientColorKey>(
                    gradientColorKeys.Length,
                    allocator,
                    MemoryOptions.UninitializedMemory
                );

                fixed (GradientColorKey* items = &gradient.colorKeys[0])
                {
                    _gradientColorKeys.CopyFrom(items, gradientColorKeys.Length);
                }
            }

            public Color Evaluate(float time)
            {
                var keyLeft = _gradientColorKeys[0];
                var keyRight = _gradientColorKeys[_gradientColorKeys.Length - 1];

                for (var i = 0; i < _gradientColorKeys.Length; i++)
                {
                    if (_gradientColorKeys[i].time < time)
                    {
                        keyLeft = _gradientColorKeys[i];
                    }

                    if (_gradientColorKeys[i].time > time)
                    {
                        keyRight = _gradientColorKeys[i];
                        break;
                    }
                }

                if (_gradientMode == GradientMode.Blend)
                {
                    var blendTime = Mathf.InverseLerp(keyLeft.time, keyRight.time, time);
                    return Color.Lerp(keyLeft.color, keyRight.color, blendTime);
                }

                return keyRight.color;
            }

            public void Dispose()
            {
                _gradientColorKeys.Dispose();
            }
        }

        #endregion

        [WriteOnly] private NativeArray<Color32> _texture;
        private NativeGradient _nativeGradient;
        private readonly int _resolution;

        public GradientEvaluate(Gradient gradient, NativeArray<Color32> texture, int resolution)
        {
            _nativeGradient = new NativeGradient(gradient, Allocator.TempJob);
            _texture = texture;
            _resolution = resolution;
        }

        public void Execute(int index)
        {
            _texture[index] = _nativeGradient.Evaluate(index / (_resolution - 1f));
        }

        public void Dispose()
        {
            _nativeGradient.Dispose();
        }
    }
}