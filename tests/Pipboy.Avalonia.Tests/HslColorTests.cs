using Avalonia.Media;
using Xunit;

namespace Pipboy.Avalonia.Tests;

public class HslColorTests
{
    [Fact]
    public void FromColor_PureRed_CorrectHue()
    {
        var hsl = HslColor.FromColor(Colors.Red);
        Assert.Equal(0f, hsl.H, 1);
        Assert.Equal(1f, hsl.S, 2);
        Assert.Equal(0.5f, hsl.L, 2);
    }

    [Fact]
    public void FromColor_PureGreen_CorrectHue()
    {
        var hsl = HslColor.FromColor(Colors.Lime);
        Assert.Equal(120f, hsl.H, 1);
        Assert.Equal(1f, hsl.S, 2);
        Assert.Equal(0.5f, hsl.L, 2);
    }

    [Fact]
    public void FromColor_PureBlue_CorrectHue()
    {
        var hsl = HslColor.FromColor(Colors.Blue);
        Assert.Equal(240f, hsl.H, 1);
        Assert.Equal(1f, hsl.S, 2);
        Assert.Equal(0.5f, hsl.L, 2);
    }

    [Fact]
    public void FromColor_White_LightnessOne()
    {
        var hsl = HslColor.FromColor(Colors.White);
        Assert.Equal(1f, hsl.L, 2);
        Assert.Equal(0f, hsl.S, 2);
    }

    [Fact]
    public void FromColor_Black_LightnessZero()
    {
        var hsl = HslColor.FromColor(Colors.Black);
        Assert.Equal(0f, hsl.L, 2);
    }

    [Fact]
    public void ToColor_RoundTrip_IsApproximatelyEqual()
    {
        var original = Color.Parse("#15FF52");
        var hsl = HslColor.FromColor(original);
        var roundTripped = hsl.ToColor();

        // Allow ±2 per channel for floating point rounding
        Assert.InRange(roundTripped.R, original.R - 2, original.R + 2);
        Assert.InRange(roundTripped.G, original.G - 2, original.G + 2);
        Assert.InRange(roundTripped.B, original.B - 2, original.B + 2);
    }

    [Fact]
    public void AdjustLightness_Increase_LighterColor()
    {
        var hsl = new HslColor(120f, 1f, 0.5f);
        var lighter = hsl.AdjustLightness(0.25f);
        Assert.Equal(0.75f, lighter.L, 2);
        Assert.Equal(hsl.H, lighter.H, 2);
        Assert.Equal(hsl.S, lighter.S, 2);
    }

    [Fact]
    public void AdjustLightness_Decrease_DarkerColor()
    {
        var hsl = new HslColor(120f, 1f, 0.5f);
        var darker = hsl.AdjustLightness(-0.25f);
        Assert.Equal(0.25f, darker.L, 2);
    }

    [Fact]
    public void AdjustLightness_ClampedToZero()
    {
        var hsl = new HslColor(120f, 1f, 0.1f);
        var result = hsl.AdjustLightness(-0.5f);
        Assert.Equal(0f, result.L, 2);
    }

    [Fact]
    public void AdjustLightness_ClampedToOne()
    {
        var hsl = new HslColor(120f, 1f, 0.9f);
        var result = hsl.AdjustLightness(0.5f);
        Assert.Equal(1f, result.L, 2);
    }

    [Fact]
    public void WithLightness_RetainsHueAndSaturation()
    {
        var hsl = new HslColor(200f, 0.8f, 0.5f);
        var result = hsl.WithLightness(0.3f);
        Assert.Equal(200f, result.H, 1);
        Assert.Equal(0.8f, result.S, 2);
        Assert.Equal(0.3f, result.L, 2);
    }

    [Fact]
    public void Constructor_ClampsHOutOfRange()
    {
        var hsl = new HslColor(400f, 0.5f, 0.5f);
        Assert.Equal(360f, hsl.H, 1);

        var hsl2 = new HslColor(-10f, 0.5f, 0.5f);
        Assert.Equal(0f, hsl2.H, 1);
    }

    [Fact]
    public void Constructor_ClampsSAndLOutOfRange()
    {
        var hsl = new HslColor(180f, 2f, -0.5f);
        Assert.Equal(1f, hsl.S, 2);
        Assert.Equal(0f, hsl.L, 2);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = new HslColor(120f, 0.8f, 0.5f);
        var b = new HslColor(120f, 0.8f, 0.5f);
        Assert.Equal(a, b);
        Assert.True(a == b);
    }
}
