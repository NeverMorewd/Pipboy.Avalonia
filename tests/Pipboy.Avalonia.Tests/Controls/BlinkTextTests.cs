using Xunit;

namespace Pipboy.Avalonia.Tests;

public class BlinkTextTests
{
    [Fact]
    public void IsBlinking_Default_IsFalse()
    {
        var blink = new BlinkText();
        Assert.False(blink.IsBlinking);
    }

    [Fact]
    public void BlinkIntervalMs_Default_Is530()
    {
        var blink = new BlinkText();
        Assert.Equal(530.0, blink.BlinkIntervalMs);
    }

    [Fact]
    public void IsBlinking_CanBeSetTrue()
    {
        var blink = new BlinkText { IsBlinking = true };
        Assert.True(blink.IsBlinking);
    }

    [Fact]
    public void IsBlinking_CanBeToggled()
    {
        var blink = new BlinkText { IsBlinking = true };
        blink.IsBlinking = false;
        Assert.False(blink.IsBlinking);
    }

    [Fact]
    public void PseudoClass_Blinking_SetWhenIsBlinkingTrue()
    {
        var blink = new BlinkText { IsBlinking = true };
        Assert.Contains(":blinking", blink.Classes);
    }

    [Fact]
    public void PseudoClass_Blinking_ClearedWhenIsBlinkingFalse()
    {
        var blink = new BlinkText { IsBlinking = true };
        blink.IsBlinking = false;
        Assert.DoesNotContain(":blinking", blink.Classes);
    }

    [Fact]
    public void BlinkIntervalMs_CanBeChanged()
    {
        var blink = new BlinkText { BlinkIntervalMs = 1000 };
        Assert.Equal(1000, blink.BlinkIntervalMs);
    }
}
