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

    /// <summary>Minimum CIE L* gap <see cref="PipboyPaletteStrategy.ColorblindSafe"/> enforces between adjacent severity tiers.</summary>
    private const double MinSeverityLStarSeparation = 15.0;

    /// <summary>
    /// Extra headroom <see cref="ApplySeverityLightnessSeparation"/> targets beyond
    /// <see cref="MinSeverityLStarSeparation"/>, absorbing the ~0.1-0.3 L* quantization noise
    /// <see cref="PerceptualLightness.WithCieLStar"/> can leave once its result is rounded to
    /// 8-bit RGB - without it, a target of exactly 15.0 can land fractionally under 15.0.
    /// </summary>
    private const double SeverityLStarSafetyMargin = 1.0;

    // --- Primary shades ---
    public Color Primary { get; }
    public Color PrimaryLight { get; }
    public Color PrimaryDark { get; }

    // --- Background surfaces (very dark) ---
    // Private setters (rather than plain get-only) so the AccessibleContrast/HighContrast/
    // PerceptuallyUniform/ColorblindSafe strategies below can adjust a role after its Classic
    // value is computed, from a helper method instead of one long constructor body.
    public Color Background { get; private set; }
    public Color Surface { get; private set; }
    public Color SurfaceHigh { get; private set; }

    // --- Text ---
    public Color Text { get; private set; }
    public Color TextDim { get; private set; }

    // --- Interactive states ---
    public Color Hover { get; private set; }
    public Color Pressed { get; private set; }
    public Color Disabled { get; private set; }

    // --- Focus and selection ---
    public Color Focus { get; private set; }
    public Color Selection { get; }

    // --- Semantic status (same hue as primary, varying lightness) ---
    public Color Error { get; private set; }
    public Color Warning { get; private set; }
    public Color Success { get; private set; }

    // --- Borders ---
    public Color Border { get; private set; }
    public Color BorderFocus { get; private set; }

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

        switch (strategy)
        {
            case PipboyPaletteStrategy.AccessibleContrast:
                ApplyContrastFloor(textMinContrast: 4.5, uiMinContrast: 3.0); // WCAG 1.4.3 / 1.4.11
                break;

            case PipboyPaletteStrategy.HighContrast:
                ApplyContrastFloor(textMinContrast: 7.0, uiMinContrast: 4.5); // WCAG 1.4.6 (text); non-text floor is Pipboy's own, stricter than 1.4.11's 3:1
                break;

            case PipboyPaletteStrategy.PerceptuallyUniform:
                ApplyPerceptualLightnessRamp();
                break;

            case PipboyPaletteStrategy.ColorblindSafe:
                ApplySeverityLightnessSeparation();
                break;
        }
    }

    /// <summary>Backs <see cref="PipboyPaletteStrategy.AccessibleContrast"/> and <see cref="PipboyPaletteStrategy.HighContrast"/> - same roles, different floors.</summary>
    private void ApplyContrastFloor(double textMinContrast, double uiMinContrast)
    {
        Text = WcagContrast.EnsureMinimumContrast(Text, Surface, textMinContrast);
        TextDim = WcagContrast.EnsureMinimumContrast(TextDim, Surface, textMinContrast);
        Success = WcagContrast.EnsureMinimumContrast(Success, Surface, textMinContrast);
        Warning = WcagContrast.EnsureMinimumContrast(Warning, Surface, textMinContrast);
        Error = WcagContrast.EnsureMinimumContrast(Error, Surface, textMinContrast);

        Border = WcagContrast.EnsureMinimumContrast(Border, Surface, uiMinContrast);
        BorderFocus = WcagContrast.EnsureMinimumContrast(BorderFocus, Surface, uiMinContrast);
        Focus = WcagContrast.EnsureMinimumContrast(Focus, Surface, uiMinContrast);
    }

    /// <summary>
    /// Backs <see cref="PipboyPaletteStrategy.PerceptuallyUniform"/>. Target CIE L* values are
    /// anchored to what the Classic derivation already produces for the default Pipboy green
    /// primary (#15FF52) - so the default theme looks effectively unchanged, while every other
    /// hue is corrected to match its perceived brightness instead of drifting with raw HSL.
    /// </summary>
    private void ApplyPerceptualLightnessRamp()
    {
        Background = PerceptualLightness.WithCieLStar(new HslColor(Background), 6.5);
        Surface = PerceptualLightness.WithCieLStar(new HslColor(Surface), 10.1);
        SurfaceHigh = PerceptualLightness.WithCieLStar(new HslColor(SurfaceHigh), 15.0);
        Text = PerceptualLightness.WithCieLStar(new HslColor(Text), 93.8);
        TextDim = PerceptualLightness.WithCieLStar(new HslColor(TextDim), 68.1);
        Hover = PerceptualLightness.WithCieLStar(new HslColor(Hover), 29.2);
        Pressed = PerceptualLightness.WithCieLStar(new HslColor(Pressed), 17.1);
        Disabled = PerceptualLightness.WithCieLStar(new HslColor(Disabled), 41.0);
    }

    /// <summary>
    /// Backs <see cref="PipboyPaletteStrategy.ColorblindSafe"/>: widens the CIE L* gap between
    /// the three severity tiers to at least <see cref="MinSeverityLStarSeparation"/> if the
    /// Classic derivation left them closer than that, working from Success upward so each step
    /// only ever gets lighter, never fighting the adjustment before it.
    /// </summary>
    private void ApplySeverityLightnessSeparation()
    {
        var successL = PerceptualLightness.ToCieLStar(Success);
        var warningL = PerceptualLightness.ToCieLStar(Warning);
        var errorL = PerceptualLightness.ToCieLStar(Error);

        if (warningL - successL >= MinSeverityLStarSeparation && errorL - warningL >= MinSeverityLStarSeparation)
            return; // already spaced out - most non-green hues already clear this on their own

        // Pushing Warning/Error upward alone runs out of room for primaries whose Classic
        // Success/Error already sit close to white (green's severity ramp compresses into the
        // top ~10 points of L* - see ColorblindSafe_WidensGreenPrimarysCompressedSeverityTiers).
        // Re-center the three tiers around their original midpoint instead, spaced apart by
        // MinSeverityLStarSeparation plus a safety margin, then slide that window to fit within
        // valid CIE L* (0-100) rather than letting Error clip past white.
        var targetGap = MinSeverityLStarSeparation + SeverityLStarSafetyMargin;
        var center = (successL + errorL) / 2.0;
        center = Math.Clamp(center, targetGap, 100.0 - targetGap);

        Success = PerceptualLightness.WithCieLStar(new HslColor(Success), center - targetGap);
        Warning = PerceptualLightness.WithCieLStar(new HslColor(Warning), center);
        Error = PerceptualLightness.WithCieLStar(new HslColor(Error), center + targetGap);
    }
}
