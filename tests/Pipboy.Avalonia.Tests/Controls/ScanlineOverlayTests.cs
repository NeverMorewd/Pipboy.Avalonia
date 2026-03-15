using Xunit;

namespace Pipboy.Avalonia.Tests;

public class ScanlineOverlayTests
{
    [Fact]
    public void LineSpacing_Default_IsFour()
    {
        var overlay = new ScanlineOverlay();
        Assert.Equal(4.0, overlay.LineSpacing);
    }

    [Fact]
    public void LineOpacity_Default_Is0point08()
    {
        var overlay = new ScanlineOverlay();
        Assert.Equal(0.08, overlay.LineOpacity);
    }

    [Fact]
    public void LineSpacing_CanBeChanged()
    {
        var overlay = new ScanlineOverlay { LineSpacing = 8.0 };
        Assert.Equal(8.0, overlay.LineSpacing);
    }

    [Fact]
    public void LineOpacity_CanBeChanged()
    {
        var overlay = new ScanlineOverlay { LineOpacity = 0.15 };
        Assert.Equal(0.15, overlay.LineOpacity);
    }

    [Fact]
    public void LineOpacity_Zero_DoesNotThrow()
    {
        var overlay = new ScanlineOverlay { LineOpacity = 0.0 };
        Assert.Equal(0.0, overlay.LineOpacity);
    }
}
