using System;
using PCG.Terrain.Core.DataTypes;
using UniRx;

namespace PCG.Terrain.Common.Extensions
{
    public static class SliderValueExtensions
    {
        public static void OnValueChangedSetText(this SliderValue sliderValue, float minValue, float maxValue,
            Func<float, string> selector, bool wholeNumbers = false)
        {
            sliderValue.Slider.minValue = minValue;
            sliderValue.Slider.maxValue = maxValue;
            sliderValue.Slider.wholeNumbers = wholeNumbers;
            sliderValue.Slider.OnValueChangedAsObservable().SubscribeToText(sliderValue.Text, selector);
        }

        public static void SetValue(this SliderValue sliderValue, float value)
        {
            sliderValue.Slider.value = value;
        }
    }
}