using System;
using Avalonia.Media;

namespace Pipboy.Avalonia;

/// <summary>
/// CIE L* (perceptual lightness, 0-100) math, used by
/// <see cref="PipboyPaletteStrategy.PerceptuallyUniform"/> and
/// <see cref="PipboyPaletteStrategy.ColorblindSafe"/>. Plain HSL lightness is not perceptually
/// uniform - a saturated yellow and a saturated blue at the same HSL L look noticeably
/// different in brightness - which is exactly the gap CIE L* (the same lightness axis Material
/// Design 3's HCT/tonal palettes and most OKLCH-based design-token systems build on) closes.
/// </summary>
public static class PerceptualLightness
{
    /// <summary>The CIE L* (0-100) of a color, derived from the same WCAG relative luminance used by <see cref="WcagContrast"/>.</summary>
    public static double ToCieLStar(Color color) => FromRelativeLuminance(WcagContrast.RelativeLuminance(color));

    /// <summary>CIE L* from a relative luminance already in the [0, 1] range (the standard piecewise CIE 1976 formula).</summary>
    public static double FromRelativeLuminance(double relativeLuminance)
    {
        const double epsilon = 216.0 / 24389.0; // (6/29)^3
        const double kappa = 24389.0 / 27.0;    // (29/3)^3
        return relativeLuminance > epsilon
            ? (116.0 * Math.Cbrt(relativeLuminance)) - 16.0
            : kappa * relativeLuminance;
    }

    /// <summary>
    /// The color with <paramref name="source"/>'s hue, saturation, and alpha whose CIE L*
    /// matches <paramref name="targetLStar"/> (0-100) as closely as HSL lightness allows -
    /// found by binary search, since CIE L* increases monotonically with HSL lightness for a
    /// fixed hue/saturation.
    /// </summary>
    public static Color WithCieLStar(HslColor source, double targetLStar)
    {
        var lo = 0.0;
        var hi = 1.0;
        var best = source;

        for (var i = 0; i < 24; i++)
        {
            var mid = (lo + hi) / 2.0;
            var candidate = new HslColor(source.A, source.H, source.S, mid);
            best = candidate;

            if (ToCieLStar(candidate.ToRgb()) < targetLStar) lo = mid; else hi = mid;
        }

        return best.ToRgb();
    }
}
