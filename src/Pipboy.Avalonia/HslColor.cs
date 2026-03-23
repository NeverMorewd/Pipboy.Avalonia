using System;
using Avalonia.Media;

namespace Pipboy.Avalonia;

/// <summary>
///  Extension methods for <see cref="HslColor"/>.
/// </summary>
public static class HslColorExtensions
{
    extension(HslColor color)
    {
        public HslColor AdjustLightness(double delta)
            => color.WithLightness(color.L + delta);

        public HslColor WithLightness(double lightness) =>
            new(color.A, color.H, color.S, lightness < 0 ? 0 :
                lightness > 1 ? 1 : lightness);
    }
}