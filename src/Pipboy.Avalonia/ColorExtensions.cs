using Avalonia.Media;
using System;

namespace Pipboy.Avalonia;

public static class ColorExtensions
{
    public static Color WithAlpha(this Color color, byte alpha)
    {
        return Color.FromArgb(alpha, color.R, color.G, color.B);
    }

    public static Color WithOpacity(this Color color, double opacity)
    {
        byte alpha = (byte)(Math.Clamp(opacity, 0, 1) * 255);
        return Color.FromArgb(alpha, color.R, color.G, color.B);
    }
}
