using System;

namespace PCG.Terrain.Core.DataTypes
{
    [Flags]
    public enum CalculationIndicator : byte
    {
        Free = 0x00,
        Busy = 0x01
    }

    public static class CalculationIndicators
    {
        public const CalculationIndicator Default = CalculationIndicator.Free;
    }
}