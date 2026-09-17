using System;
using Avalonia.Media;

namespace Pipboy.Avalonia;

/// <summary>
/// WCAG 2.x contrast-ratio math (relative luminance + the (L1+0.05)/(L2+0.05) formula from
/// <see href="https://www.w3.org/TR/WCAG21/#dfn-contrast-ratio">WCAG 2.1</see>), shared by
/// <see cref="PipboyColorPalette"/>'s <see cref="PipboyPaletteStrategy.AccessibleContrast"/>
/// strategy and available to consumers who need to check their own color choices.
/// </summary>
public static class WcagContrast
{
    /// <summary>Contrast ratio between two colors, in the range [1, 21]. Alpha is ignored.</summary>
    public static double Ratio(Color first, Color second)
    {
        var firstLuminance = RelativeLuminance(first);
        var secondLuminance = RelativeLuminance(second);
        var lighter = Math.Max(firstLuminance, secondLuminance);
        var darker = Math.Min(firstLuminance, secondLuminance);
        return (lighter + 0.05) / (darker + 0.05);
    }

    /// <summary>The WCAG relative luminance of a color, in the range [0, 1].</summary>
    public static double RelativeLuminance(Color color)
    {
        static double Linearize(byte channel)
        {
            var normalized = channel / 255.0;
            return normalized <= 0.04045
                ? normalized / 12.92
                : Math.Pow((normalized + 0.055) / 1.055, 2.4);
        }

        return (0.2126 * Linearize(color.R))
             + (0.7152 * Linearize(color.G))
             + (0.0722 * Linearize(color.B));
    }

    /// <summary>
    /// Returns <paramref name="foreground"/> unchanged if it already meets
    /// <paramref name="minRatio"/> against <paramref name="background"/>; otherwise nudges its
    /// HSL lightness - toward white or black, whichever increases contrast - until it does,
    /// preserving hue and saturation exactly. This is the same "keep the hue, move the
    /// lightness" adjustment <see cref="PipboyColorPalette"/> already uses for every other
    /// role, just aimed at a target contrast ratio instead of a fixed value.
    /// </summary>
    public static Color EnsureMinimumContrast(Color foreground, Color background, double minRatio)
    {
        if (Ratio(foreground, background) >= minRatio)
            return foreground;

        var hsl = new HslColor(foreground);
        var backgroundIsDark = RelativeLuminance(background) < 0.5;

        // Backgrounds in this palette are always near-black, so lightening is virtually always
        // the answer - but branching on the actual luminance keeps this correct even for a
        // caller that passes a light background.
        var lo = backgroundIsDark ? hsl.L : 0.0;
        var hi = backgroundIsDark ? 1.0 : hsl.L;

        // The extremes bound the search: if even the most extreme lightness in the chosen
        // direction can't reach minRatio, no lightness can - return it rather than looping.
        var best = new HslColor(hsl.A, hsl.H, hsl.S, backgroundIsDark ? hi : lo).ToRgb();
        if (Ratio(best, background) < minRatio)
            return best;

        // Binary search for the least extreme lightness that still clears the target, so the
        // result stays as close to the original color as the contrast requirement allows.
        for (var i = 0; i < 24; i++)
        {
            var mid = (lo + hi) / 2.0;
            var candidate = new HslColor(hsl.A, hsl.H, hsl.S, mid).ToRgb();
            var meets = Ratio(candidate, background) >= minRatio;

            if (backgroundIsDark)
            {
                if (meets) { best = candidate; hi = mid; } else { lo = mid; }
            }
            else
            {
                if (meets) { best = candidate; lo = mid; } else { hi = mid; }
            }
        }

        return best;
    }
}
