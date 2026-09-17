using System;
using Avalonia.Media;

namespace Pipboy.Avalonia;

/// <summary>
/// Represents a complete monochromatic color palette derived from a single primary color.
/// All colors keep the primary hue while their saturation and lightness are tuned for their role.
/// </summary>
public sealed class PipboyColorPalette
{
    private const double SaturationThreshold = 0.25;

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

    /// <summary>Equivalent to <see cref="PipboyColorPalette(Color, PipboyPaletteStrategy)"/> with <see cref="PipboyPaletteStrategy.Classic"/>.</summary>
    public PipboyColorPalette(Color primaryColor) : this(primaryColor, PipboyPaletteStrategy.Classic)
    {
    }

    public PipboyColorPalette(Color primaryColor, PipboyPaletteStrategy strategy)
    {
        var hsl = new HslColor(primaryColor); //HslColor.FromColor(primaryColor);

        Primary = primaryColor;

        // Lighter / darker primary shades
        PrimaryLight = hsl.AdjustLightness(0.25f).ToRgb();
        PrimaryDark = hsl.AdjustLightness(-0.25f).ToRgb();

        // Scale the role saturation for muted primaries without introducing a hue cast
        // for grey/slate themes. Primaries with S >= the threshold use each role's full
        // saturation; an achromatic primary produces an achromatic derived palette.
        double saturationScale = Math.Min(hsl.S / SaturationThreshold, 1.0);

        HslColor Derived(double saturation, double lightness) =>
            new(hsl.A, hsl.H, saturation * saturationScale, lightness);

        // The surface ramp is intentionally close to black. This keeps panels calm while
        // retaining enough separation for the hierarchy to remain visible.
        Background = Derived(0.55, 0.06).ToRgb();
        Surface = Derived(0.49, 0.085).ToRgb();
        SurfaceHigh = Derived(0.45, 0.115).ToRgb();

        // Softer text saturation avoids the white-on-neon look while the lightness values
        // keep body text readable on every surface.
        Text = Derived(0.35, 0.915).ToRgb();
        TextDim = Derived(0.15, 0.62).ToRgb();

        // Interactive states — fixed dark lightness so all hues (green, yellow,
        // cyan, orange…) stay dark enough for Text to remain readable.
        Hover = Derived(0.55, 0.20).ToRgb();
        Pressed = Derived(0.45, 0.13).ToRgb();
        Disabled = Derived(0.15, 0.35).ToRgb();

        // Focus / selection
        double focusL = hsl.L + 0.30f;
        Focus = hsl.WithLightness(focusL > 0.95f ? 0.95f : focusL).ToRgb();

        double selL = hsl.L * 0.45f;
        Selection = new HslColor(hsl.A, hsl.H, hsl.S, selL > 0.30f ? 0.30f : selL).ToRgb();

        // Borders are deliberately quieter than the phosphor primary. Using a darker,
        // less saturated tone prevents every control from reading as a heavy neon frame.
        Border = Derived(0.31, 0.21).ToRgb();
        double bfL = Math.Min(hsl.L + 0.12f, 0.78f);
        BorderFocus = hsl.WithLightness(bfL).ToRgb();

        // Semantic status — same hue as primary, varying lightness for severity tiers.
        // Keeps the monochromatic design principle: Success (mid), Warning (bright), Error (near-white).
        // The saturation scale collapses saturation to 0 for gray/achromatic primaries.
        double semS = Math.Min(hsl.S * 1.1, 0.95) * saturationScale;
        Success = new HslColor(hsl.A, hsl.H, semS, 0.60f).ToRgb();
        Warning = new HslColor(hsl.A, hsl.H, semS, 0.78f).ToRgb();
        Error   = new HslColor(hsl.A, hsl.H, semS, 0.93f).ToRgb();

        if (strategy == PipboyPaletteStrategy.AccessibleContrast)
        {
            const double TextMinContrast = 4.5;    // WCAG 1.4.3 (Contrast Minimum)
            const double UiMinContrast = 3.0;       // WCAG 1.4.11 (Non-text Contrast)

            Text = WcagContrast.EnsureMinimumContrast(Text, Surface, TextMinContrast);
            TextDim = WcagContrast.EnsureMinimumContrast(TextDim, Surface, TextMinContrast);
            Success = WcagContrast.EnsureMinimumContrast(Success, Surface, TextMinContrast);
            Warning = WcagContrast.EnsureMinimumContrast(Warning, Surface, TextMinContrast);
            Error = WcagContrast.EnsureMinimumContrast(Error, Surface, TextMinContrast);

            Border = WcagContrast.EnsureMinimumContrast(Border, Surface, UiMinContrast);
            BorderFocus = WcagContrast.EnsureMinimumContrast(BorderFocus, Surface, UiMinContrast);
            Focus = WcagContrast.EnsureMinimumContrast(Focus, Surface, UiMinContrast);
        }
    }
}
