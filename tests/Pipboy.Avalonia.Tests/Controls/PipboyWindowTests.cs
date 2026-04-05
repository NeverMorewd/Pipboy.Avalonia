using Xunit;

namespace Pipboy.Avalonia.Tests;

/// <summary>
/// Tests for PipboyWindow StyledProperty registrations.
/// These use property metadata rather than live instances so no Avalonia
/// platform / windowing backend is required.
/// </summary>
public class PipboyWindowTests
{
    // ── TitleBarHeight ──────────────────────────────────────────────────────────────

    [Fact]
    public void TitleBarHeightProperty_DefaultValue_Is32()
    {
        var defaultValue = PipboyWindow.TitleBarHeightProperty
            .GetDefaultValue(typeof(PipboyWindow));
        Assert.Equal(32.0, defaultValue);
    }

    [Fact]
    public void TitleBarHeightProperty_Name_IsTitleBarHeight()
    {
        Assert.Equal(nameof(PipboyWindow.TitleBarHeight),
                     PipboyWindow.TitleBarHeightProperty.Name);
    }

    [Fact]
    public void TitleBarHeightProperty_OwnerType_IsPipboyWindow()
    {
        Assert.Equal(typeof(PipboyWindow),
                     PipboyWindow.TitleBarHeightProperty.OwnerType);
    }

    // ── TitleBarIcon ────────────────────────────────────────────────────────────────

    [Fact]
    public void TitleBarIconProperty_DefaultValue_IsNull()
    {
        var defaultValue = PipboyWindow.TitleBarIconProperty
            .GetDefaultValue(typeof(PipboyWindow));
        Assert.Null(defaultValue);
    }

    [Fact]
    public void TitleBarIconProperty_Name_IsTitleBarIcon()
    {
        Assert.Equal(nameof(PipboyWindow.TitleBarIcon),
                     PipboyWindow.TitleBarIconProperty.Name);
    }

    [Fact]
    public void TitleBarIconProperty_OwnerType_IsPipboyWindow()
    {
        Assert.Equal(typeof(PipboyWindow),
                     PipboyWindow.TitleBarIconProperty.OwnerType);
    }

    // ── TitleBarContent ─────────────────────────────────────────────────────────────

    [Fact]
    public void TitleBarContentProperty_DefaultValue_IsNull()
    {
        var defaultValue = PipboyWindow.TitleBarContentProperty
            .GetDefaultValue(typeof(PipboyWindow));
        Assert.Null(defaultValue);
    }

    [Fact]
    public void TitleBarContentProperty_Name_IsTitleBarContent()
    {
        Assert.Equal(nameof(PipboyWindow.TitleBarContent),
                     PipboyWindow.TitleBarContentProperty.Name);
    }

    [Fact]
    public void TitleBarContentProperty_OwnerType_IsPipboyWindow()
    {
        Assert.Equal(typeof(PipboyWindow),
                     PipboyWindow.TitleBarContentProperty.OwnerType);
    }
}
