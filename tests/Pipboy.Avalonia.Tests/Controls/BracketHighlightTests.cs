using Xunit;

namespace Pipboy.Avalonia.Tests;

public class BracketHighlightTests
{
    [Fact]
    public void IsSelected_Default_IsFalse()
    {
        var bh = new BracketHighlight();
        Assert.False(bh.IsSelected);
    }

    [Fact]
    public void IsSelected_CanBeSetTrue()
    {
        var bh = new BracketHighlight { IsSelected = true };
        Assert.True(bh.IsSelected);
    }

    [Fact]
    public void PseudoClass_Selected_SetWhenIsSelectedTrue()
    {
        var bh = new BracketHighlight { IsSelected = true };
        Assert.Contains(":selected", bh.Classes);
    }

    [Fact]
    public void PseudoClass_Selected_ClearedWhenIsSelectedFalse()
    {
        var bh = new BracketHighlight { IsSelected = true };
        bh.IsSelected = false;
        Assert.DoesNotContain(":selected", bh.Classes);
    }
}
