using Avalonia.Media;
using Xunit;

namespace Pipboy.Avalonia.Tests;

public class CrtDisplayTests
{
    // ── Default property values ───────────────────────────────────────────────────────

    [Fact]
    public void Defaults_EnableScanlines_IsTrue()
        => Assert.True(new CrtDisplay().EnableScanlines);

    [Fact]
    public void Defaults_EnableScanlineAnimation_IsTrue()
        => Assert.True(new CrtDisplay().EnableScanlineAnimation);

    [Fact]
    public void Defaults_EnableScanBeam_IsTrue()
        => Assert.True(new CrtDisplay().EnableScanBeam);

    [Fact]
    public void Defaults_EnableNoise_IsTrue()
        => Assert.True(new CrtDisplay().EnableNoise);

    [Fact]
    public void Defaults_EnableVignette_IsTrue()
        => Assert.True(new CrtDisplay().EnableVignette);

    [Fact]
    public void Defaults_EnableFlicker_IsFalse()
        => Assert.False(new CrtDisplay().EnableFlicker);

    [Fact]
    public void Defaults_ShowFps_IsFalse()
        => Assert.False(new CrtDisplay().ShowFps);

    [Fact]
    public void Defaults_ScanlineSpacing_IsThree()
        => Assert.Equal(3.0, new CrtDisplay().ScanlineSpacing);

    [Fact]
    public void Defaults_ScanlineOpacity_IsInRange()
    {
        double opacity = new CrtDisplay().ScanlineOpacity;
        Assert.InRange(opacity, 0.0, 1.0);
    }

    [Fact]
    public void Defaults_NoiseDensity_IsInRange()
    {
        double density = new CrtDisplay().NoiseDensity;
        Assert.InRange(density, 0.0, 1.0);
    }

    [Fact]
    public void Defaults_VignetteIntensity_IsInRange()
    {
        double intensity = new CrtDisplay().VignetteIntensity;
        Assert.InRange(intensity, 0.0, 1.0);
    }

    [Fact]
    public void Defaults_NoiseRefreshIntervalMs_IsPositive()
        => Assert.True(new CrtDisplay().NoiseRefreshIntervalMs > 0);

    // ── Property round-trips ──────────────────────────────────────────────────────────

    [Fact]
    public void Property_EnableScanlines_RoundTrips()
    {
        var c = new CrtDisplay { EnableScanlines = false };
        Assert.False(c.EnableScanlines);
    }

    [Fact]
    public void Property_ScanlineColor_RoundTrips()
    {
        var color = Color.FromArgb(255, 0, 200, 100);
        var c = new CrtDisplay { ScanlineColor = color };
        Assert.Equal(color, c.ScanlineColor);
    }

    [Fact]
    public void Property_NoiseDensity_RoundTrips()
    {
        var c = new CrtDisplay { NoiseDensity = 0.05 };
        Assert.Equal(0.05, c.NoiseDensity);
    }

    [Fact]
    public void Property_VignetteIntensity_RoundTrips()
    {
        var c = new CrtDisplay { VignetteIntensity = 0.6 };
        Assert.Equal(0.6, c.VignetteIntensity);
    }

    [Fact]
    public void Property_EnableScanBeamGradient_RoundTrips()
    {
        var c = new CrtDisplay { EnableScanBeamGradient = false };
        Assert.False(c.EnableScanBeamGradient);
    }

    // ── PositiveMod utility ───────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0.0,  3.0, 0.0)]
    [InlineData(1.0,  3.0, 1.0)]
    [InlineData(3.0,  3.0, 0.0)]   // exactly one period → 0
    [InlineData(4.5,  3.0, 1.5)]
    [InlineData(-0.5, 3.0, 2.5)]   // negative input → positive result
    [InlineData(-3.0, 3.0, 0.0)]
    [InlineData(6.0,  3.0, 0.0)]
    public void PositiveMod_ReturnsCorrectValue(double x, double m, double expected)
        => Assert.Equal(expected, CrtDisplay.PositiveMod(x, m), precision: 9);

    [Fact]
    public void PositiveMod_ZeroModulus_ReturnsZero()
        => Assert.Equal(0.0, CrtDisplay.PositiveMod(5.0, 0.0));

    [Fact]
    public void PositiveMod_NegativeModulus_ReturnsZero()
        => Assert.Equal(0.0, CrtDisplay.PositiveMod(5.0, -1.0));

    [Fact]
    public void PositiveMod_ResultAlwaysInRange()
    {
        for (double x = -20; x <= 20; x += 0.7)
        {
            double result = CrtDisplay.PositiveMod(x, 3.0);
            Assert.InRange(result, 0.0, 3.0);
        }
    }

    // ── ScanBeamColor default has non-zero alpha ──────────────────────────────────────

    [Fact]
    public void Defaults_ScanBeamColor_HasAlpha()
    {
        var c = new CrtDisplay();
        Assert.True(c.ScanBeamColor.A > 0, "ScanBeamColor alpha should be > 0 so the beam is visible.");
        Assert.True(c.ScanBeamColor.A < 255, "ScanBeamColor should be semi-transparent.");
    }
}
