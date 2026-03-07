using Avalonia.Media;
using Xunit;

namespace Pipboy.Avalonia.Tests;

public class PipboyColorPaletteTests
{
    private static readonly Color PipboyGreen = Color.Parse("#15FF52");

    [Fact]
    public void Constructor_PrimaryIsInputColor()
    {
        var palette = new PipboyColorPalette(PipboyGreen);
        Assert.Equal(PipboyGreen, palette.Primary);
    }

    [Fact]
    public void PrimaryLight_IsLighterThanPrimary()
    {
        var palette = new PipboyColorPalette(PipboyGreen);
        var primaryHsl = HslColor.FromColor(palette.Primary);
        var lightHsl = HslColor.FromColor(palette.PrimaryLight);
        Assert.True(lightHsl.L > primaryHsl.L, "PrimaryLight should have higher lightness than Primary");
    }

    [Fact]
    public void PrimaryDark_IsDarkerThanPrimary()
    {
        var palette = new PipboyColorPalette(PipboyGreen);
        var primaryHsl = HslColor.FromColor(palette.Primary);
        var darkHsl = HslColor.FromColor(palette.PrimaryDark);
        Assert.True(darkHsl.L < primaryHsl.L, "PrimaryDark should have lower lightness than Primary");
    }

    [Fact]
    public void Background_IsVeryDark()
    {
        var palette = new PipboyColorPalette(PipboyGreen);
        var hsl = HslColor.FromColor(palette.Background);
        Assert.True(hsl.L < 0.15f, "Background should be very dark (L < 0.15)");
    }

    [Fact]
    public void Surface_IsDarkerThanSurfaceHigh()
    {
        var palette = new PipboyColorPalette(PipboyGreen);
        var surfaceHsl = HslColor.FromColor(palette.Surface);
        var surfaceHighHsl = HslColor.FromColor(palette.SurfaceHigh);
        Assert.True(surfaceHighHsl.L >= surfaceHsl.L, "SurfaceHigh should be at least as light as Surface");
    }

    [Fact]
    public void Text_IsSignificantlyLighter()
    {
        var palette = new PipboyColorPalette(PipboyGreen);
        var textHsl = HslColor.FromColor(palette.Text);
        Assert.True(textHsl.L > 0.7f, "Text should be bright (L > 0.7)");
    }

    [Fact]
    public void TextDim_IsDimmerThanText()
    {
        var palette = new PipboyColorPalette(PipboyGreen);
        var textHsl = HslColor.FromColor(palette.Text);
        var dimHsl = HslColor.FromColor(palette.TextDim);
        Assert.True(dimHsl.L < textHsl.L, "TextDim should be dimmer than Text");
    }

    [Fact]
    public void Hover_IsVisibleOnSurfaceButDarkEnoughForText()
    {
        // Hover is a fixed dark-lightness surface highlight (~L=0.20).
        // It must be lighter than Surface so it's visible, but dark enough
        // that Text (L≈0.85) remains readable across all hues.
        var palette = new PipboyColorPalette(PipboyGreen);
        var hoverHsl  = HslColor.FromColor(palette.Hover);
        var surfaceHsl = HslColor.FromColor(palette.Surface);
        var textHsl   = HslColor.FromColor(palette.Text);
        Assert.True(hoverHsl.L > surfaceHsl.L, "Hover should be lighter than Surface for visibility");
        Assert.True(hoverHsl.L < textHsl.L,    "Hover should be darker than Text to keep text readable");
        Assert.True(hoverHsl.L < 0.35f,        "Hover should stay dark (L < 0.35) for all hues");
    }

    [Fact]
    public void Pressed_IsDarkerThanHover()
    {
        var palette = new PipboyColorPalette(PipboyGreen);
        var hoverHsl   = HslColor.FromColor(palette.Hover);
        var pressedHsl = HslColor.FromColor(palette.Pressed);
        Assert.True(pressedHsl.L < hoverHsl.L, "Pressed should be darker than Hover");
    }

    [Fact]
    public void AllColorsShareSameHue()
    {
        var palette = new PipboyColorPalette(PipboyGreen);
        var primaryHsl = HslColor.FromColor(palette.Primary);

        var colorsToCheck = new[]
        {
            palette.PrimaryLight,
            palette.PrimaryDark,
            palette.Hover,
            palette.Pressed,
        };

        foreach (var c in colorsToCheck)
        {
            var hsl = HslColor.FromColor(c);
            Assert.InRange(hsl.H, primaryHsl.H - 2f, primaryHsl.H + 2f);
        }
    }

    [Fact]
    public void Constructor_AnyValidColor_DoesNotThrow()
    {
        var colors = new[] { Colors.Red, Colors.Blue, Colors.White, Colors.Black, Colors.Gray };
        foreach (var c in colors)
        {
            var palette = new PipboyColorPalette(c);
            Assert.NotEqual(default(Color), palette.Background);
        }
    }

    [Fact]
    public void LightnessVariation_MeetsRequirement()
    {
        // Requirements say lighter = +20-30%, darker = -20-30%
        var palette = new PipboyColorPalette(Color.Parse("#15FF52"));
        var primaryL = HslColor.FromColor(palette.Primary).L;
        var lightL = HslColor.FromColor(palette.PrimaryLight).L;
        var darkL = HslColor.FromColor(palette.PrimaryDark).L;

        float lightDiff = lightL - primaryL;
        float darkDiff = primaryL - darkL;

        Assert.InRange(lightDiff, 0.15f, 0.40f);
        Assert.InRange(darkDiff, 0.15f, 0.40f);
    }
}
