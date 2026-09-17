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
}
