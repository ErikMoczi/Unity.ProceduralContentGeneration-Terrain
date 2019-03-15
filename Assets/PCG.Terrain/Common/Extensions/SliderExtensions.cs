using System;
using UniRx;
using UnityEngine.UI;

namespace PCG.Terrain.Common.Extensions
{
    public static class SliderExtensions
    {
        public static IObservable<T> AsObservable<T>(this Slider slider, Func<float, T> selector)
        {
            return slider.onValueChanged.AsObservable().Select(selector);
        }
    }
}