using System;
using Avalonia.Media;

namespace Pipboy.Avalonia;

/// <summary>
/// Represents a complete monochromatic color palette derived from a single primary color.
/// All colors share the same hue and saturation, varying only in lightness.
/// </summary>
public sealed class PipboyColorPalette
{
    // --- Primary shades ---
    public Color Primary { get; }
    public Color PrimaryLight { get; }
    public Color PrimaryDark { get; }

    // --- Background surfaces (very dark) ---
    public Color Background { get; }
    public Color Surface { get; }
    public Color SurfaceHigh { get; }

    // --- Text ---
    public Color Text { get; }
    public Color TextDim { get; }

    // --- Interactive states ---
    public Color Hover { get; }
    public Color Pressed { get; }
    public Color Disabled { get; }

    // --- Focus and selection ---
    public Color Focus { get; }
    public Color Selection { get; }

    // --- Semantic status (same hue as primary, varying lightness) ---
    public Color Error { get; }
    public Color Warning { get; }
    public Color Success { get; }

    // --- Borders ---
    public Color Border { get; }
    public Color BorderFocus { get; }

    public PipboyColorPalette(Color primaryColor)
    {
        var hsl = new HslColor(primaryColor); //HslColor.FromColor(primaryColor);

        Primary = primaryColor;

        // Lighter / darker primary shades
        PrimaryLight = hsl.AdjustLightness(0.25f).ToRgb();
        PrimaryDark = hsl.AdjustLightness(-0.25f).ToRgb();

        // Saturation scale: primaries with low saturation (greys, slates) produce
        // proportionally desaturated derived colors so there is no unwanted hue cast.
        // Primaries with S ≥ 0.25 get the full target saturation; achromatic primaries
        // (S = 0) produce a pure grey palette.
        double ss = Math.Min(hsl.S / 0.25, 1.0);

        // Dark backgrounds — same hue, low saturation, very low lightness
        Background = new HslColor(hsl.A, hsl.H, 0.30f * ss, 0.05f).ToRgb();
        Surface    = new HslColor( hsl.A, hsl.H, 0.28f * ss, 0.09f).ToRgb();
        SurfaceHigh = new HslColor(hsl.A, hsl.H, 0.25f * ss, 0.14f).ToRgb();

        // Text — same hue, moderately saturated, high lightness
        Text    = new HslColor(hsl.A, hsl.H, 0.70f * ss, 0.85f).ToRgb();
        TextDim = new HslColor( hsl.A, hsl.H, 0.45f * ss, 0.58f).ToRgb();

        // Interactive states — fixed dark lightness so all hues (green, yellow,
        // cyan, orange…) stay dark enough for Text (L=0.85) to be readable.
        Hover    = new HslColor(hsl.A, hsl.H, Math.Min(hsl.S * 0.60f, 0.55f), 0.20f).ToRgb();
        Pressed  = new HslColor(hsl.A, hsl.H, Math.Min(hsl.S * 0.50f, 0.45f), 0.13f).ToRgb();
        Disabled = new HslColor(hsl.A, hsl.H, 0.15f * ss, 0.35f).ToRgb();

        // Focus / selection
        double focusL = hsl.L + 0.30f;
        Focus = hsl.WithLightness(focusL > 0.95f ? 0.95f : focusL).ToRgb();

        double selL = hsl.L * 0.45f;
        Selection = new HslColor(hsl.A, hsl.H, hsl.S, selL > 0.30f ? 0.30f : selL).ToRgb();

        // Borders
        Border = hsl.AdjustLightness(-0.10f).ToRgb();
        double bfL = hsl.L + 0.25f;
        BorderFocus = hsl.WithLightness(bfL > 0.90f ? 0.90f : bfL).ToRgb();

        // Semantic status — same hue as primary, varying lightness for severity tiers.
        // Keeps the monochromatic design principle: Success (mid), Warning (bright), Error (near-white).
        // The ss factor collapses saturation to 0 for gray/achromatic primaries.
        double semS = Math.Min(hsl.S * 1.1f * ss, 0.95f);
        Success = new HslColor(hsl.A, hsl.H, semS, 0.60f).ToRgb();
        Warning = new HslColor(hsl.A, hsl.H, semS, 0.78f).ToRgb();
        Error   = new HslColor(hsl.A, hsl.H, semS, 0.93f).ToRgb();
    }
}
