using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace PCG.Terrain.Core.DataTypes
{
    [Serializable]
    public struct SliderValue
    {
#pragma warning disable 649
        [SerializeField, Required] private Slider slider;
        [SerializeField, Required] private Text text;
#pragma warning restore 649

        public Slider Slider => slider;
        public Text Text => text;
    }
}