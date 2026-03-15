using Xunit;

namespace Pipboy.Avalonia.Tests;

public class TerminalPanelTests
{
    [Fact]
    public void TypewriterEffect_Default_IsFalse()
    {
        var panel = new TerminalPanel();
        Assert.False(panel.TypewriterEffect);
    }

    [Fact]
    public void TypewriterDelayMs_Default_Is30()
    {
        var panel = new TerminalPanel();
        Assert.Equal(30.0, panel.TypewriterDelayMs);
    }

    [Fact]
    public void TypewriterEffect_CanBeEnabled()
    {
        var panel = new TerminalPanel { TypewriterEffect = true };
        Assert.True(panel.TypewriterEffect);
    }

    [Fact]
    public void DisplayedText_EmptyByDefault()
    {
        var panel = new TerminalPanel();
        Assert.Equal(string.Empty, panel.DisplayedText);
    }

    [Fact]
    public void DisplayedText_EqualsContent_WhenTypewriterDisabled()
    {
        var panel = new TerminalPanel { TypewriterEffect = false, Content = "HELLO VAULT" };
        Assert.Equal("HELLO VAULT", panel.DisplayedText);
    }

    [Fact]
    public void DisplayedText_InitiallyEmpty_WhenTypewriterEnabled()
    {
        var panel = new TerminalPanel { TypewriterEffect = true, Content = "HELLO VAULT" };
        Assert.Equal(string.Empty, panel.DisplayedText);
    }

    [Fact]
    public void DisplayedText_EmptyWhenContentIsNotString()
    {
        var panel = new TerminalPanel { TypewriterEffect = true, Content = 42 };
        Assert.Equal(string.Empty, panel.DisplayedText);
    }
}
