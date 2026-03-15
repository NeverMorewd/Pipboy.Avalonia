using Xunit;

namespace Pipboy.Avalonia.Tests;

public class RatedAttributeTests
{
    [Fact]
    public void Dots_Count_MatchesMaximum()
    {
        var attr = new RatedAttribute { Maximum = 10, Value = 5 };
        Assert.Equal(10, attr.Dots.Count);
    }

    [Fact]
    public void Dots_FilledCount_MatchesValue()
    {
        var attr = new RatedAttribute { Maximum = 10, Value = 7 };
        int filled = 0;
        foreach (var d in attr.Dots) if (d.IsFilled) filled++;
        Assert.Equal(7, filled);
    }

    [Fact]
    public void Value_Clamped_ToMaximum()
    {
        var attr = new RatedAttribute { Maximum = 10, Value = 15 };
        Assert.True(attr.Value <= attr.Maximum);
    }

    [Fact]
    public void Value_Clamped_ToZero()
    {
        var attr = new RatedAttribute { Maximum = 10, Value = -3 };
        Assert.True(attr.Value >= 0);
    }

    [Fact]
    public void Maximum_Default_IsTen()
    {
        var attr = new RatedAttribute();
        Assert.Equal(10, attr.Maximum);
    }

    [Fact]
    public void Maximum_Change_RebuildsDots()
    {
        var attr = new RatedAttribute { Maximum = 10, Value = 5 };
        attr.Maximum = 7;
        Assert.Equal(7, attr.Dots.Count);
    }

    [Fact]
    public void Maximum_MinimumIsOne()
    {
        var attr = new RatedAttribute { Maximum = 0, Value = 1 };
        Assert.True(attr.Dots.Count >= 1);
    }

    [Fact]
    public void Label_CanBeSet()
    {
        var attr = new RatedAttribute { Label = "STRENGTH" };
        Assert.Equal("STRENGTH", attr.Label);
    }
}
