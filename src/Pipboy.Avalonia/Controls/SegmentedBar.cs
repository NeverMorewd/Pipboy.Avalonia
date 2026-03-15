using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace Pipboy.Avalonia;

/// <summary>
/// A Pip-Boy style status bar made of discrete rectangular segments.
/// Ideal for HP, AP, RAD and other game stat displays.
/// </summary>
public class SegmentedBar : TemplatedControl
{
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<SegmentedBar, double>(nameof(Value), defaultValue: 0.0);

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<SegmentedBar, double>(nameof(Maximum), defaultValue: 100.0);

    public static readonly StyledProperty<int> SegmentCountProperty =
        AvaloniaProperty.Register<SegmentedBar, int>(nameof(SegmentCount), defaultValue: 10);

    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<SegmentedBar, string>(nameof(Label), defaultValue: string.Empty);

    public static readonly DirectProperty<SegmentedBar, IReadOnlyList<SegmentItem>> SegmentsProperty =
        AvaloniaProperty.RegisterDirect<SegmentedBar, IReadOnlyList<SegmentItem>>(
            nameof(Segments), o => o.Segments);

    private IReadOnlyList<SegmentItem> _segments = Array.Empty<SegmentItem>();

    static SegmentedBar()
    {
        ValueProperty.Changed.AddClassHandler<SegmentedBar>((x, _) => x.RebuildSegments());
        MaximumProperty.Changed.AddClassHandler<SegmentedBar>((x, _) => x.RebuildSegments());
        SegmentCountProperty.Changed.AddClassHandler<SegmentedBar>((x, _) => x.RebuildSegments());
    }

    public SegmentedBar() => RebuildSegments();

    /// <summary>Gets or sets the current value.</summary>
    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>Gets or sets the maximum value.</summary>
    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>Gets or sets the number of discrete segments to display.</summary>
    public int SegmentCount
    {
        get => GetValue(SegmentCountProperty);
        set => SetValue(SegmentCountProperty, value);
    }

    /// <summary>Gets or sets the optional label shown above the bar.</summary>
    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Gets the computed list of segment states used by the template.</summary>
    public IReadOnlyList<SegmentItem> Segments
    {
        get => _segments;
        private set => SetAndRaise(SegmentsProperty, ref _segments, value);
    }

    private void RebuildSegments()
    {
        int count = Math.Max(1, SegmentCount);
        double max = Maximum > 0 ? Maximum : 1.0;
        double val = Math.Clamp(Value, 0, max);
        int filledCount = (int)Math.Round(val / max * count);

        var items = new SegmentItem[count];
        for (int i = 0; i < count; i++)
            items[i] = new SegmentItem(i < filledCount);

        Segments = items;
    }
}
