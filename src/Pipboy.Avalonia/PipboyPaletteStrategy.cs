namespace Pipboy.Avalonia;

/// <summary>
/// Controls how <see cref="PipboyColorPalette"/> derives its roles from a primary color.
/// Set via <see cref="PipboyThemeManager.SetPaletteStrategy"/>.
/// </summary>
public enum PipboyPaletteStrategy
{
    /// <summary>
    /// The original fixed-lightness derivation. Some roles - most notably
    /// <see cref="PipboyColorPalette.Border"/>, deliberately kept quiet so borders don't read
    /// as a heavy neon frame - fall below WCAG's 3:1 minimum contrast for non-text UI
    /// components against the surface they sit on. Default, for exact backward compatibility.
    /// </summary>
    Classic,

    /// <summary>
    /// Same hue-preserving derivation as <see cref="Classic"/>, but every role that can render
    /// as foreground content - text, focus indicators, borders, semantic status colors - is
    /// nudged in lightness afterward (hue and saturation untouched) until it meets WCAG 2.1's
    /// contrast minimum against the surface it's drawn on: 4.5:1 for text
    /// (<see href="https://www.w3.org/TR/WCAG21/#contrast-minimum">SC 1.4.3</see>), 3:1 for
    /// non-text UI components (<see href="https://www.w3.org/TR/WCAG21/#non-text-contrast">SC
    /// 1.4.11</see>). A role that already clears its target is left untouched.
    /// </summary>
    AccessibleContrast,

    /// <summary>
    /// The same roles <see cref="AccessibleContrast"/> checks, held to a stricter floor: 7:1 for
    /// text (WCAG's AAA text threshold,
    /// <see href="https://www.w3.org/TR/WCAG21/#contrast-enhanced">SC 1.4.6</see>) and 4.5:1 for
    /// non-text UI components (WCAG defines no AAA tier for non-text contrast; this is simply a
    /// stricter floor than <see cref="AccessibleContrast"/>'s 3:1, in the spirit of an OS-level
    /// "high contrast" accessibility mode for low-vision users).
    /// </summary>
    HighContrast,

    /// <summary>
    /// Recomputes the background/surface/text ramp (<c>Background</c>, <c>Surface</c>,
    /// <c>SurfaceHigh</c>, <c>Text</c>, <c>TextDim</c>, <c>Hover</c>, <c>Pressed</c>,
    /// <c>Disabled</c>) so each role has the same CIE L* (perceptual lightness) regardless of
    /// the chosen primary hue, instead of the same raw HSL lightness. Plain HSL lightness is
    /// not perceptually uniform - the same technique underlies Material Design 3's HCT/tonal
    /// palettes and most OKLCH-based design-token systems (Radix Colors, Tailwind v4's default
    /// palette). Every other role is unchanged from <see cref="Classic"/>.
    /// </summary>
    PerceptuallyUniform,

    /// <summary>
    /// Same derivation as <see cref="Classic"/>, but widens the CIE L* gap between
    /// <c>Success</c>, <c>Warning</c>, and <c>Error</c> to at least 15 points (of 100) if it
    /// isn't already, so the three remain distinguishable by lightness alone - the standard
    /// mitigation dataviz-focused color systems (ColorBrewer, Highcharts, IBM Carbon) use
    /// instead of a full color-vision-deficiency simulation, which this does not perform. It
    /// does not change hue, so it does not help someone who can perceive the hue difference
    /// fine but the palette's default lightness spacing was too close to tell tiers apart at a
    /// glance.
    /// </summary>
    ColorblindSafe,
}
