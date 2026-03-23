using Avalonia.Media;
using Xunit;

namespace Pipboy.Avalonia.Tests;

public class HslColorTests
{
    [Fact]
    public void FromColor_PureRed_CorrectHue()
    {
        var hsl = new HslColor(Colors.Red); // HslColor.FromColor(Colors.Red);
        Assert.Equal(0f, hsl.H, 1);
        Assert.Equal(1f, hsl.S, 2);
        Assert.Equal(0.5f, hsl.L, 2);
    }

    [Fact]
    public void FromColor_PureGreen_CorrectHue()
    {
        var hsl = new HslColor(Colors.Lime);  //HslColor.FromColor(Colors.Lime);
        Assert.Equal(120f, hsl.H, 1);
        Assert.Equal(1f, hsl.S, 2);
        Assert.Equal(0.5f, hsl.L, 2);
    }

    [Fact]
    public void FromColor_PureBlue_CorrectHue()
    {
        var hsl = new HslColor(Colors.Blue); //HslColor.FromColor(Colors.Blue);
        Assert.Equal(240f, hsl.H, 1);
        Assert.Equal(1f, hsl.S, 2);
        Assert.Equal(0.5f, hsl.L, 2);
    }

    [Fact]
    public void FromColor_White_LightnessOne()
    {
        var hsl = new HslColor(Colors.White); //HslColor.FromColor(Colors.White);
        Assert.Equal(1f, hsl.L, 2);
        Assert.Equal(0f, hsl.S, 2);
    }

    [Fact]
    public void FromColor_Black_LightnessZero()
    {
        var hsl = new HslColor(Colors.Black); //HslColor.FromColor(Colors.Black);
        Assert.Equal(0f, hsl.L, 2);
    }

    [Fact]
    public void ToColor_RoundTrip_IsApproximatelyEqual()
    {
        var original = Color.Parse("#15FF52");
        var hsl = new HslColor(original); //HslColor.FromColor(original);
        var roundTripped = hsl.ToRgb();

        // Allow ±2 per channel for floating point rounding
        Assert.InRange(roundTripped.R, original.R - 2, original.R + 2);
        Assert.InRange(roundTripped.G, original.G - 2, original.G + 2);
        Assert.InRange(roundTripped.B, original.B - 2, original.B + 2);
    }

    [Fact]
    public void AdjustLightness_Increase_LighterColor()
    {
        var hsl = new HslColor(1, 120, 1, 0.5);
        var lighter = hsl.AdjustLightness(0.25);
        Assert.Equal(0.75, lighter.L, 2);
        Assert.Equal(hsl.H, lighter.H, 2);
        Assert.Equal(hsl.S, lighter.S, 2);
    }

    [Fact]
    public void AdjustLightness_Decrease_DarkerColor()
    {
        var hsl = new HslColor(1, 120, 1, 0.5);
        var darker = hsl.AdjustLightness(-0.25);
        Assert.Equal(0.25, darker.L, 2);
    }

    [Fact]
    public void AdjustLightness_ClampedToZero()
    {
        var hsl = new HslColor(1, 120, 1, 0.1);
        var result = hsl.AdjustLightness(-0.5);
        Assert.Equal(0f, result.L, 2);
    }

    [Fact]
    public void AdjustLightness_ClampedToOne()
    {
        var hsl = new HslColor(1, 120, 1, 0.9);
        var result = hsl.AdjustLightness(0.5);
        Assert.Equal(1, result.L, 2);
    }

    [Fact]
    public void WithLightness_RetainsHueAndSaturation()
    {
        var hsl = new HslColor(1, 200, 0.8, 0.5);
        var result = hsl.WithLightness(0.3);
        Assert.Equal(200, result.H, 1);
        Assert.Equal(0.8, result.S, 2);
        Assert.Equal(0.3, result.L, 2);
    }

    [Fact]
    public void Constructor_NormalizesHIntoRange()
    {
        // H wraps into [0, 360) — 400 % 360 = 40
        var hsl = new HslColor(1, 400, 0.5, 0.5);
        Assert.Equal(40, hsl.H, 1);

        // 360 itself wraps to 0
        var hsl360 = new HslColor(1, 360, 0.5, 0.5);
        Assert.Equal(0, hsl360.H, 1);

        // Negative wraps: -10 + 360 = 350
        var hsl2 = new HslColor(1, -10, 0.5, 0.5);
        Assert.Equal(350, hsl2.H, 1);
    }

    [Fact]
    public void Constructor_ClampsSAndLOutOfRange()
    {
        var hsl = new HslColor(1, 180, 2, -0.5);
        Assert.Equal(1, hsl.S, 2);
        Assert.Equal(0, hsl.L, 2);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = new HslColor(1, 120, 0.8, 0.5);
        var b = new HslColor(1, 120, 0.8, 0.5);
        Assert.Equal(a, b);
        Assert.True(a == b);
    }
}
