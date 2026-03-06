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

    // --- Semantic status (fixed hues, not derived from primary) ---
    public Color Error { get; }
    public Color Warning { get; }
    public Color Success { get; }

    // --- Borders ---
    public Color Border { get; }
    public Color BorderFocus { get; }

    public PipboyColorPalette(Color primaryColor)
    {
        var hsl = HslColor.FromColor(primaryColor);

        Primary = primaryColor;

        // Lighter / darker primary shades (requirement: +/- 20-30% lightness)
        PrimaryLight = hsl.AdjustLightness(0.25f).ToColor();
        PrimaryDark = hsl.AdjustLightness(-0.25f).ToColor();

        // Dark backgrounds — same hue, low saturation, very low lightness
        Background = new HslColor(hsl.H, 0.30f, 0.05f).ToColor();
        Surface = new HslColor(hsl.H, 0.28f, 0.09f).ToColor();
        SurfaceHigh = new HslColor(hsl.H, 0.25f, 0.14f).ToColor();

        // Text — same hue, moderately saturated, high lightness
        Text = new HslColor(hsl.H, 0.70f, 0.85f).ToColor();
        TextDim = new HslColor(hsl.H, 0.45f, 0.58f).ToColor();

        // Interactive states
        Hover = hsl.AdjustLightness(0.12f).ToColor();
        Pressed = hsl.AdjustLightness(-0.12f).ToColor();
        Disabled = new HslColor(hsl.H, 0.15f, 0.35f).ToColor();

        // Focus / selection
        float focusL = hsl.L + 0.30f;
        Focus = hsl.WithLightness(focusL > 0.95f ? 0.95f : focusL).ToColor();

        float selL = hsl.L * 0.45f;
        Selection = new HslColor(hsl.H, hsl.S, selL > 0.30f ? 0.30f : selL).ToColor();

        // Borders
        Border = hsl.AdjustLightness(-0.10f).ToColor();
        float bfL = hsl.L + 0.25f;
        BorderFocus = hsl.WithLightness(bfL > 0.90f ? 0.90f : bfL).ToColor();

        // Semantic status — fixed conventional hues regardless of primary color
        Error   = Color.Parse("#FF4040");
        Warning = Color.Parse("#FFAA00");
        Success = Color.Parse("#40C840");
    }
}
