using System;
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
        var primaryHsl = new HslColor(palette.Primary);
        var lightHsl = new HslColor(palette.PrimaryLight);
        Assert.True(lightHsl.L > primaryHsl.L, "PrimaryLight should have higher lightness than Primary");
    }

    [Fact]
    public void PrimaryDark_IsDarkerThanPrimary()
    {
        var palette = new PipboyColorPalette(PipboyGreen);
        var primaryHsl = new HslColor(palette.Primary);
        var darkHsl = new HslColor(palette.PrimaryDark);
        Assert.True(darkHsl.L < primaryHsl.L, "PrimaryDark should have lower lightness than Primary");
    }

    [Fact]
    public void Background_IsVeryDark()
    {
        var palette = new PipboyColorPalette(PipboyGreen);
        var hsl = new HslColor(palette.Background);
        Assert.True(hsl.L < 0.15f, "Background should be very dark (L < 0.15)");
    }

    [Fact]
    public void Surface_IsDarkerThanSurfaceHigh()
    {
        var palette = new PipboyColorPalette(PipboyGreen);
        var surfaceHsl = new HslColor(palette.Surface);
        var surfaceHighHsl = new HslColor(palette.SurfaceHigh);
        Assert.True(surfaceHighHsl.L >= surfaceHsl.L, "SurfaceHigh should be at least as light as Surface");
    }

    [Fact]
    public void Text_IsSignificantlyLighter()
    {
        var palette = new PipboyColorPalette(PipboyGreen);
        var textHsl = new HslColor(palette.Text);
        Assert.True(textHsl.L > 0.7f, "Text should be bright (L > 0.7)");
    }

    [Fact]
    public void TextDim_IsDimmerThanText()
    {
        var palette = new PipboyColorPalette(PipboyGreen);
        var textHsl = new HslColor(palette.Text);
        var dimHsl = new HslColor(palette.TextDim);
        Assert.True(dimHsl.L < textHsl.L, "TextDim should be dimmer than Text");
    }

    [Fact]
    public void SurfaceRamp_IsQuietAndOrdered()
    {
        var palette = new PipboyColorPalette(PipboyGreen);
        var background = new HslColor(palette.Background);
        var surface = new HslColor(palette.Surface);
        var surfaceHigh = new HslColor(palette.SurfaceHigh);

        Assert.InRange(background.L, 0.05, 0.07);
        Assert.InRange(surface.L, 0.075, 0.10);
        Assert.InRange(surfaceHigh.L, 0.10, 0.13);
        Assert.True(background.L < surface.L && surface.L < surfaceHigh.L,
            "Surface tones should form a steadily lighter hierarchy");
    }

    [Fact]
    public void TextAndBorder_HaveContrastAgainstSurface()
    {
        var palette = new PipboyColorPalette(PipboyGreen);

        Assert.True(ContrastRatio(palette.Text, palette.Surface) >= 7.0,
            "Primary text should have strong contrast against the surface");
        Assert.True(ContrastRatio(palette.TextDim, palette.Surface) >= 4.5,
            "Dim text should remain readable against the surface");
        Assert.True(ContrastRatio(palette.Border, palette.Surface) >= 1.5,
            "Borders should remain visible without competing with text");
    }

    [Fact]
    public void Hover_IsVisibleOnSurfaceButDarkEnoughForText()
    {
        // Hover is a fixed dark-lightness surface highlight (~L=0.20).
        // It must be lighter than Surface so it's visible, but dark enough
        // that Text (L≈0.85) remains readable across all hues.
        var palette = new PipboyColorPalette(PipboyGreen);
        var hoverHsl  = new HslColor(palette.Hover);
        var surfaceHsl = new HslColor(palette.Surface);
        var textHsl   = new HslColor(palette.Text);
        Assert.True(hoverHsl.L > surfaceHsl.L, "Hover should be lighter than Surface for visibility");
        Assert.True(hoverHsl.L < textHsl.L,    "Hover should be darker than Text to keep text readable");
        Assert.True(hoverHsl.L < 0.35f,        "Hover should stay dark (L < 0.35) for all hues");
    }

    [Fact]
    public void Pressed_IsDarkerThanHover()
    {
        var palette = new PipboyColorPalette(PipboyGreen);
        var hoverHsl   = new HslColor(palette.Hover);
        var pressedHsl = new HslColor(palette.Pressed);
        Assert.True(pressedHsl.L < hoverHsl.L, "Pressed should be darker than Hover");
    }

    [Fact]
    public void AllColorsShareSameHue()
    {
        var palette = new PipboyColorPalette(PipboyGreen);
        var primaryHsl = new HslColor(palette.Primary);

        var colorsToCheck = new[]
        {
            palette.PrimaryLight,
            palette.PrimaryDark,
            palette.Hover,
            palette.Pressed,
        };

        foreach (var c in colorsToCheck)
        {
            var hsl = new HslColor(c);
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
        var primaryL = new HslColor(palette.Primary).L;
        var lightL = new HslColor(palette.PrimaryLight).L;
        var darkL = new HslColor(palette.PrimaryDark).L;

        double lightDiff = lightL - primaryL;
        double darkDiff = primaryL - darkL;

        Assert.InRange(lightDiff, 0.15f, 0.40f);
        Assert.InRange(darkDiff, 0.15f, 0.40f);
    }

    private static double ContrastRatio(Color first, Color second) => WcagContrast.Ratio(first, second);

    [Fact]
    public void Classic_BorderContrast_IsUnaffectedByAccessibleContrastExisting()
    {
        // Documents the gap AccessibleContrast exists to fix: Classic's Border is deliberately
        // quiet and does not clear the 3:1 non-text contrast minimum on its own.
        var palette = new PipboyColorPalette(PipboyGreen, PipboyPaletteStrategy.Classic);
        Assert.True(ContrastRatio(palette.Border, palette.Surface) < 3.0);
    }

    [Theory]
    [InlineData("#15FF52")] // default Pipboy green
    [InlineData("#FF0000")] // red
    [InlineData("#3050FF")] // blue - low default saturation scaling territory
    [InlineData("#808080")] // achromatic
    public void AccessibleContrast_MeetsWcagMinimumsAgainstSurface(string hex)
    {
        var palette = new PipboyColorPalette(Color.Parse(hex), PipboyPaletteStrategy.AccessibleContrast);

        Assert.True(ContrastRatio(palette.Text, palette.Surface) >= 4.5,
            "Text must meet WCAG 1.4.3 (4.5:1) against Surface");
        Assert.True(ContrastRatio(palette.TextDim, palette.Surface) >= 4.5,
            "TextDim must meet WCAG 1.4.3 (4.5:1) against Surface");
        Assert.True(ContrastRatio(palette.Success, palette.Surface) >= 4.5,
            "Success must meet WCAG 1.4.3 (4.5:1) against Surface");
        Assert.True(ContrastRatio(palette.Warning, palette.Surface) >= 4.5,
            "Warning must meet WCAG 1.4.3 (4.5:1) against Surface");
        Assert.True(ContrastRatio(palette.Error, palette.Surface) >= 4.5,
            "Error must meet WCAG 1.4.3 (4.5:1) against Surface");

        Assert.True(ContrastRatio(palette.Border, palette.Surface) >= 3.0,
            "Border must meet WCAG 1.4.11 (3:1) against Surface");
        Assert.True(ContrastRatio(palette.BorderFocus, palette.Surface) >= 3.0,
            "BorderFocus must meet WCAG 1.4.11 (3:1) against Surface");
        Assert.True(ContrastRatio(palette.Focus, palette.Surface) >= 3.0,
            "Focus must meet WCAG 1.4.11 (3:1) against Surface");
    }

    [Fact]
    public void AccessibleContrast_PreservesHue()
    {
        var palette = new PipboyColorPalette(PipboyGreen, PipboyPaletteStrategy.AccessibleContrast);
        var primaryHue = new HslColor(palette.Primary).H;

        foreach (var color in new[] { palette.Text, palette.TextDim, palette.Border, palette.BorderFocus, palette.Focus })
        {
            var hue = new HslColor(color).H;
            Assert.InRange(hue, primaryHue - 2f, primaryHue + 2f);
        }
    }

    [Fact]
    public void AccessibleContrast_LeavesAlreadyCompliantRolesUnchanged()
    {
        // Text/TextDim already clear their targets under Classic (see
        // TextAndBorder_HaveContrastAgainstSurface above) - AccessibleContrast should not move
        // them at all, only the roles that actually fail.
        var classic = new PipboyColorPalette(PipboyGreen, PipboyPaletteStrategy.Classic);
        var accessible = new PipboyColorPalette(PipboyGreen, PipboyPaletteStrategy.AccessibleContrast);

        Assert.Equal(classic.Text, accessible.Text);
        Assert.Equal(classic.TextDim, accessible.TextDim);
    }

    [Theory]
    [InlineData("#15FF52")]
    [InlineData("#FF0000")]
    [InlineData("#3050FF")]
    [InlineData("#808080")]
    public void HighContrast_MeetsStricterMinimumsAgainstSurface(string hex)
    {
        var palette = new PipboyColorPalette(Color.Parse(hex), PipboyPaletteStrategy.HighContrast);

        foreach (var text in new[] { palette.Text, palette.TextDim, palette.Success, palette.Warning, palette.Error })
            Assert.True(ContrastRatio(text, palette.Surface) >= 7.0, "HighContrast text roles must meet WCAG 1.4.6 (7:1)");

        foreach (var ui in new[] { palette.Border, palette.BorderFocus, palette.Focus })
            Assert.True(ContrastRatio(ui, palette.Surface) >= 4.5, "HighContrast non-text roles must meet the stricter 4.5:1 floor");
    }

    [Fact]
    public void HighContrast_IsAtLeastAsStrictAsAccessibleContrast()
    {
        var accessible = new PipboyColorPalette(PipboyGreen, PipboyPaletteStrategy.AccessibleContrast);
        var high = new PipboyColorPalette(PipboyGreen, PipboyPaletteStrategy.HighContrast);

        Assert.True(ContrastRatio(high.Text, high.Surface) >= ContrastRatio(accessible.Text, accessible.Surface));
        Assert.True(ContrastRatio(high.Border, high.Surface) >= ContrastRatio(accessible.Border, accessible.Surface));
    }

    [Theory]
    [InlineData("#FFD700")] // yellow - high HSL lightness perception even at low L
    [InlineData("#3050FF")] // blue - low HSL lightness perception even at moderate L
    public void PerceptuallyUniform_RampMatchesGreenBaselinePerceptually(string hex)
    {
        // The strategy is anchored to what Classic already produces for the default green -
        // so every hue's ramp should land close to the same CIE L* the green baseline has,
        // rather than drifting with raw HSL lightness the way Classic does.
        var baseline = new PipboyColorPalette(PipboyGreen, PipboyPaletteStrategy.Classic);
        var palette = new PipboyColorPalette(Color.Parse(hex), PipboyPaletteStrategy.PerceptuallyUniform);

        double LStar(Color c) => PerceptualLightness.ToCieLStar(c);

        Assert.InRange(LStar(palette.Background), LStar(baseline.Background) - 1.0, LStar(baseline.Background) + 1.0);
        Assert.InRange(LStar(palette.Text), LStar(baseline.Text) - 1.0, LStar(baseline.Text) + 1.0);
    }

    [Fact]
    public void PerceptuallyUniform_PreservesHueAndLeavesOtherRolesAlone()
    {
        var classic = new PipboyColorPalette(PipboyGreen, PipboyPaletteStrategy.Classic);
        var perceptual = new PipboyColorPalette(PipboyGreen, PipboyPaletteStrategy.PerceptuallyUniform);

        // Roles outside the background/surface/text ramp are untouched by this strategy.
        Assert.Equal(classic.Border, perceptual.Border);
        Assert.Equal(classic.Focus, perceptual.Focus);
        Assert.Equal(classic.Success, perceptual.Success);

        // A wider tolerance than the other hue-preservation checks in this file: Background in
        // particular targets CIE L* ~6.5, dark enough that 8-bit RGB quantization leaves very
        // few distinct channel values to reconstruct hue from, so a few degrees of drift here
        // is quantization noise, not a bug.
        var hue = new HslColor(classic.Primary).H;
        foreach (var c in new[] { perceptual.Background, perceptual.Surface, perceptual.Text, perceptual.TextDim })
            Assert.InRange(new HslColor(c).H, hue - 6f, hue + 6f);
    }

    [Theory]
    [InlineData("#15FF52")]
    [InlineData("#FF0000")]
    [InlineData("#3050FF")]
    public void ColorblindSafe_SeparatesSeverityTiersByLightness(string hex)
    {
        var palette = new PipboyColorPalette(Color.Parse(hex), PipboyPaletteStrategy.ColorblindSafe);

        var successL = PerceptualLightness.ToCieLStar(palette.Success);
        var warningL = PerceptualLightness.ToCieLStar(palette.Warning);
        var errorL = PerceptualLightness.ToCieLStar(palette.Error);

        Assert.True(warningL - successL >= 14.9, "Warning should be at least ~15 L* above Success");
        Assert.True(errorL - warningL >= 14.9, "Error should be at least ~15 L* above Warning");
    }

    [Fact]
    public void ColorblindSafe_LeavesAlreadySeparatedTiersUnchanged()
    {
        // Blue's Success/Warning/Error already land ~21-24 CIE L* apart under Classic (green,
        // by contrast, compresses to ~4-6 apart near the top of the scale - exactly the case
        // this strategy exists to fix) - so ColorblindSafe shouldn't move blue's tiers at all.
        var blue = Color.Parse("#3050FF");
        var classic = new PipboyColorPalette(blue, PipboyPaletteStrategy.Classic);
        var safe = new PipboyColorPalette(blue, PipboyPaletteStrategy.ColorblindSafe);

        Assert.Equal(classic.Success, safe.Success);
        Assert.Equal(classic.Warning, safe.Warning);
        Assert.Equal(classic.Error, safe.Error);
    }

    [Fact]
    public void ColorblindSafe_WidensGreenPrimarysCompressedSeverityTiers()
    {
        // Documents the gap this strategy exists to fix: the default green's Success/Warning/
        // Error compress to within a few CIE L* points of each other near the top of the
        // lightness scale, because green's luminance weight is so high that Classic's HSL
        // lightness steps barely move the resulting brightness.
        var classic = new PipboyColorPalette(PipboyGreen, PipboyPaletteStrategy.Classic);
        Assert.True(PerceptualLightness.ToCieLStar(classic.Warning) - PerceptualLightness.ToCieLStar(classic.Success) < 15.0);

        var safe = new PipboyColorPalette(PipboyGreen, PipboyPaletteStrategy.ColorblindSafe);
        Assert.NotEqual(classic.Warning, safe.Warning);
        Assert.NotEqual(classic.Error, safe.Error);
    }
}
