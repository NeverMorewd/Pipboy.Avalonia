using Xunit;

namespace Pipboy.Avalonia.Tests;

public class SegmentedBarTests
{
    [Fact]
    public void Segments_Count_MatchesSegmentCount()
    {
        var bar = new SegmentedBar { SegmentCount = 12, Value = 6, Maximum = 12 };
        Assert.Equal(12, bar.Segments.Count);
    }

    [Fact]
    public void Segments_AllFilled_WhenValueEqualsMaximum()
    {
        var bar = new SegmentedBar { SegmentCount = 10, Value = 100, Maximum = 100 };
        Assert.All(bar.Segments, s => Assert.True(s.IsFilled));
    }

    [Fact]
    public void Segments_AllEmpty_WhenValueIsZero()
    {
        var bar = new SegmentedBar { SegmentCount = 10, Value = 0, Maximum = 100 };
        Assert.All(bar.Segments, s => Assert.False(s.IsFilled));
    }

    [Fact]
    public void Segments_PartialFill_MatchesRatio()
    {
        var bar = new SegmentedBar { SegmentCount = 10, Value = 50, Maximum = 100 };
        int filled = 0;
        foreach (var s in bar.Segments) if (s.IsFilled) filled++;
        Assert.Equal(5, filled);
    }

    [Fact]
    public void Segments_FilledFirst_EmptyLast()
    {
        var bar = new SegmentedBar { SegmentCount = 10, Value = 30, Maximum = 100 };
        bool seenEmpty = false;
        foreach (var s in bar.Segments)
        {
            if (!s.IsFilled) seenEmpty = true;
            if (seenEmpty) Assert.False(s.IsFilled);
        }
    }

    [Fact]
    public void Value_AboveMaximum_ClampsToMaximum()
    {
        var bar = new SegmentedBar { SegmentCount = 10, Maximum = 50, Value = 200 };
        Assert.All(bar.Segments, s => Assert.True(s.IsFilled));
    }

    [Fact]
    public void SegmentCount_Change_RebuildsList()
    {
        var bar = new SegmentedBar { SegmentCount = 5, Value = 50, Maximum = 100 };
        Assert.Equal(5, bar.Segments.Count);
        bar.SegmentCount = 20;
        Assert.Equal(20, bar.Segments.Count);
    }

    [Fact]
    public void SegmentCount_MinimumIsOne()
    {
        var bar = new SegmentedBar { SegmentCount = 0, Value = 50, Maximum = 100 };
        Assert.True(bar.Segments.Count >= 1);
    }

    [Fact]
    public void Label_Default_IsEmpty()
    {
        var bar = new SegmentedBar();
        Assert.Equal(string.Empty, bar.Label);
    }

    // ── ShowProgressBar ───────────────────────────────────────────────────────

    [Fact]
    public void ShowProgressBar_Default_IsFalse()
    {
        var bar = new SegmentedBar();
        Assert.False(bar.ShowProgressBar);
    }

    [Fact]
    public void ShowProgressBar_CanBeSetTrue()
    {
        var bar = new SegmentedBar { ShowProgressBar = true };
        Assert.True(bar.ShowProgressBar);
    }

    [Fact]
    public void ShowProgressBar_DoesNotAffectSegments()
    {
        // Enabling the progress bar must not change the segment count or fill.
        var bar = new SegmentedBar { SegmentCount = 10, Value = 40, Maximum = 100 };
        int filledBefore = 0;
        foreach (var s in bar.Segments) if (s.IsFilled) filledBefore++;

        bar.ShowProgressBar = true;

        int filledAfter = 0;
        foreach (var s in bar.Segments) if (s.IsFilled) filledAfter++;

        Assert.Equal(filledBefore, filledAfter);
        Assert.Equal(10, bar.Segments.Count);
    }
}
