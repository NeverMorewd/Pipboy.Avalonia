using System;
using Avalonia.Media;
using Xunit;

namespace Pipboy.Avalonia.Tests;

public class PipboyThemeManagerTests
{
    [Fact]
    public void Instance_ReturnsSameObject()
    {
        var a = PipboyThemeManager.Instance;
        var b = PipboyThemeManager.Instance;
        Assert.Same(a, b);
    }

    [Fact]
    public void DefaultPrimaryColor_IsPipboyGreen()
    {
        PipboyThemeManager.Instance.ResetToDefault();
        var color = PipboyThemeManager.Instance.PrimaryColor;
        Assert.Equal(Color.Parse("#15FF52"), color);
    }

    [Fact]
    public void SetPrimaryColor_UpdatesPrimaryColor()
    {
        var manager = PipboyThemeManager.Instance;
        var newColor = Colors.Blue;
        manager.SetPrimaryColor(newColor);
        Assert.Equal(newColor, manager.PrimaryColor);
        manager.ResetToDefault();
    }

    [Fact]
    public void SetPrimaryColor_UpdatesPalette()
    {
        var manager = PipboyThemeManager.Instance;
        manager.SetPrimaryColor(Colors.Red);
        Assert.Equal(Colors.Red, manager.Palette.Primary);
        manager.ResetToDefault();
    }

    [Fact]
    public void SetPrimaryColor_SameColor_DoesNotFireEvent()
    {
        var manager = PipboyThemeManager.Instance;
        manager.ResetToDefault();
        var currentColor = manager.PrimaryColor;

        int eventCount = 0;
        EventHandler<ThemeColorChangedEventArgs> handler = (_, _) => eventCount++;
        manager.ThemeColorChanged += handler;
        try
        {
            manager.SetPrimaryColor(currentColor); // same color — should not fire
            Assert.Equal(0, eventCount);
        }
        finally
        {
            manager.ThemeColorChanged -= handler;
        }
    }

    [Fact]
    public void SetPrimaryColor_DifferentColor_FiresEvent()
    {
        var manager = PipboyThemeManager.Instance;
        manager.ResetToDefault();

        int eventCount = 0;
        ThemeColorChangedEventArgs? receivedArgs = null;
        EventHandler<ThemeColorChangedEventArgs> handler = (_, e) => { eventCount++; receivedArgs = e; };
        manager.ThemeColorChanged += handler;
        try
        {
            manager.SetPrimaryColor(Colors.Purple);
            Assert.Equal(1, eventCount);
            Assert.NotNull(receivedArgs);
            Assert.Equal(Colors.Purple, receivedArgs.Palette.Primary);
        }
        finally
        {
            manager.ThemeColorChanged -= handler;
            manager.ResetToDefault();
        }
    }

    [Fact]
    public void TrySetPrimaryColor_ValidHex_ReturnsTrue()
    {
        var manager = PipboyThemeManager.Instance;
        bool result = manager.TrySetPrimaryColor("#FF0000");
        Assert.True(result);
        manager.ResetToDefault();
    }

    [Fact]
    public void TrySetPrimaryColor_InvalidHex_ReturnsFalse()
    {
        var manager = PipboyThemeManager.Instance;
        bool result = manager.TrySetPrimaryColor("not-a-color");
        Assert.False(result);
    }

    [Fact]
    public void TrySetPrimaryColor_EmptyString_ReturnsFalse()
    {
        var manager = PipboyThemeManager.Instance;
        bool result = manager.TrySetPrimaryColor("");
        Assert.False(result);
    }

    [Fact]
    public void ResetToDefault_RestoresPipboyGreen()
    {
        var manager = PipboyThemeManager.Instance;
        manager.SetPrimaryColor(Colors.Red);
        manager.ResetToDefault();
        Assert.Equal(Color.Parse("#15FF52"), manager.PrimaryColor);
    }

    [Fact]
    public void ThemeColorChanged_EventArgs_ContainsNewPalette()
    {
        var manager = PipboyThemeManager.Instance;
        PipboyColorPalette? receivedPalette = null;
        EventHandler<ThemeColorChangedEventArgs> handler = (_, e) => receivedPalette = e.Palette;
        manager.ThemeColorChanged += handler;
        try
        {
            manager.SetPrimaryColor(Colors.Cyan);
            Assert.NotNull(receivedPalette);
            Assert.Equal(Colors.Cyan, receivedPalette.Primary);
        }
        finally
        {
            manager.ThemeColorChanged -= handler;
            manager.ResetToDefault();
        }
    }
}
