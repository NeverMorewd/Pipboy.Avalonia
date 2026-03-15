using Xunit;

namespace Pipboy.Avalonia.Tests;

public class BracketHighlightTests
{
    // ── Basic IsSelected ──────────────────────────────────────────────────────

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

    // ── SelectionGroup default ────────────────────────────────────────────────

    [Fact]
    public void SelectionGroup_Default_IsEmpty()
    {
        var bh = new BracketHighlight();
        Assert.Equal(string.Empty, bh.SelectionGroup);
    }

    [Fact]
    public void SelectionGroup_CanBeSet()
    {
        var bh = new BracketHighlight { SelectionGroup = "menu" };
        Assert.Equal("menu", bh.SelectionGroup);
    }

    // ── Mutual exclusion within group ─────────────────────────────────────────

    [Fact]
    public void SelectingOne_DeselectsOthers_InSameGroup()
    {
        const string group = "test-group-deselect";

        var a = new BracketHighlight { SelectionGroup = group, IsSelected = true };
        var b = new BracketHighlight { SelectionGroup = group };
        var c = new BracketHighlight { SelectionGroup = group };

        // Selecting b should deselect a and c.
        b.IsSelected = true;

        Assert.True(b.IsSelected);
        Assert.False(a.IsSelected);
        Assert.False(c.IsSelected);
    }

    [Fact]
    public void SelectingOne_DoesNotAffect_DifferentGroups()
    {
        var a = new BracketHighlight { SelectionGroup = "group-A", IsSelected = true };
        var b = new BracketHighlight { SelectionGroup = "group-B", IsSelected = true };

        // Selecting a again must not touch b (different group).
        a.IsSelected = false;
        a.IsSelected = true;

        Assert.True(a.IsSelected);
        Assert.True(b.IsSelected);   // b untouched
    }

    [Fact]
    public void SelectingOne_DoesNotAffect_StandaloneControls()
    {
        const string group = "test-group-standalone";

        var grouped = new BracketHighlight { SelectionGroup = group };
        var standalone = new BracketHighlight(); // no group

        standalone.IsSelected = true;
        grouped.IsSelected = true;

        // standalone has no group so it must not be touched.
        Assert.True(standalone.IsSelected);
        Assert.True(grouped.IsSelected);
    }

    [Fact]
    public void MultipleSelections_AllowedWithoutGroup()
    {
        var a = new BracketHighlight { IsSelected = true };
        var b = new BracketHighlight { IsSelected = true };

        // Without a shared group, both can be selected simultaneously.
        Assert.True(a.IsSelected);
        Assert.True(b.IsSelected);
    }

    [Fact]
    public void ChangingGroup_RemovesFromOldGroup()
    {
        const string groupOld = "test-group-old";
        const string groupNew = "test-group-new";

        var a = new BracketHighlight { SelectionGroup = groupOld, IsSelected = true };
        var b = new BracketHighlight { SelectionGroup = groupOld };

        // Move a to a different group.
        a.SelectionGroup = groupNew;

        // Selecting b should no longer affect a (they're in different groups now).
        b.IsSelected = true;

        Assert.True(b.IsSelected);
        Assert.True(a.IsSelected); // a was not deselected
    }

    [Fact]
    public void OnlyOneSelected_AtATime_InGroup()
    {
        const string group = "test-group-exclusive";

        var items = new[]
        {
            new BracketHighlight { SelectionGroup = group },
            new BracketHighlight { SelectionGroup = group },
            new BracketHighlight { SelectionGroup = group },
            new BracketHighlight { SelectionGroup = group },
        };

        // Select each in turn and verify exactly one is selected.
        foreach (var target in items)
        {
            target.IsSelected = true;

            int selectedCount = 0;
            foreach (var item in items)
                if (item.IsSelected) selectedCount++;

            Assert.Equal(1, selectedCount);
        }
    }
}
