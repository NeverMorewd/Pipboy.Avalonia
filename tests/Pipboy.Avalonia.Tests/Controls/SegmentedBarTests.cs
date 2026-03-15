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

    // ── ValueDecimalPlaces / DisplayValue / DisplayMaximum ───────────────────

    [Fact]
    public void ValueDecimalPlaces_Default_IsZero()
    {
        var bar = new SegmentedBar();
        Assert.Equal(0, bar.ValueDecimalPlaces);
    }

    [Fact]
    public void DisplayValue_Default_NoDecimals()
    {
        var bar = new SegmentedBar { Value = 73.456, Maximum = 100 };
        Assert.Equal("73", bar.DisplayValue);
    }

    [Fact]
    public void DisplayMaximum_Default_NoDecimals()
    {
        var bar = new SegmentedBar { Value = 0, Maximum = 100.0 };
        Assert.Equal("100", bar.DisplayMaximum);
    }

    [Fact]
    public void DisplayValue_OneDecimalPlace_FormatsCorrectly()
    {
        var bar = new SegmentedBar { Value = 73.456, Maximum = 100, ValueDecimalPlaces = 1 };
        Assert.Equal("73.5", bar.DisplayValue);
    }

    [Fact]
    public void DisplayValue_TwoDecimalPlaces_FormatsCorrectly()
    {
        var bar = new SegmentedBar { Value = 73.456, Maximum = 100, ValueDecimalPlaces = 2 };
        Assert.Equal("73.46", bar.DisplayValue);
    }

    [Fact]
    public void DisplayMaximum_RespectValueDecimalPlaces()
    {
        var bar = new SegmentedBar { Value = 0, Maximum = 100.5, ValueDecimalPlaces = 1 };
        Assert.Equal("100.5", bar.DisplayMaximum);
    }

    [Fact]
    public void ValueDecimalPlaces_Change_UpdatesDisplayStrings()
    {
        var bar = new SegmentedBar { Value = 78.9, Maximum = 100, ValueDecimalPlaces = 0 };
        Assert.Equal("79", bar.DisplayValue);

        bar.ValueDecimalPlaces = 2;
        Assert.Equal("78.90", bar.DisplayValue);
    }

    [Fact]
    public void ValueDecimalPlaces_Negative_TreatedAsZero()
    {
        var bar = new SegmentedBar { Value = 73.9, Maximum = 100, ValueDecimalPlaces = -3 };
        // Negative values should be clamped to 0 → no decimals
        Assert.Equal("74", bar.DisplayValue);
    }
}
