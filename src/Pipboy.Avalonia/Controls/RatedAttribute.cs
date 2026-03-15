using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace Pipboy.Avalonia;

/// <summary>
/// Displays a named attribute with a dot-based rating (S.P.E.C.I.A.L. style).
/// Example: STRENGTH ●●●●●●●○○○ 7
/// </summary>
public class RatedAttribute : TemplatedControl
{
    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<RatedAttribute, string>(nameof(Label), defaultValue: string.Empty);

    public static readonly StyledProperty<int> ValueProperty =
        AvaloniaProperty.Register<RatedAttribute, int>(nameof(Value), defaultValue: 1);

    public static readonly StyledProperty<int> MaximumProperty =
        AvaloniaProperty.Register<RatedAttribute, int>(nameof(Maximum), defaultValue: 10);

    public static readonly DirectProperty<RatedAttribute, IReadOnlyList<DotItem>> DotsProperty =
        AvaloniaProperty.RegisterDirect<RatedAttribute, IReadOnlyList<DotItem>>(
            nameof(Dots), o => o.Dots);

    private IReadOnlyList<DotItem> _dots = Array.Empty<DotItem>();

    static RatedAttribute()
    {
        ValueProperty.Changed.AddClassHandler<RatedAttribute>((x, _) => x.RebuildDots());
        MaximumProperty.Changed.AddClassHandler<RatedAttribute>((x, _) => x.RebuildDots());
    }

    public RatedAttribute() => RebuildDots();

    /// <summary>Gets or sets the attribute name (e.g. "STRENGTH").</summary>
    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Gets or sets the current attribute value.</summary>
    public int Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, Math.Clamp(value, 0, GetValue(MaximumProperty)));
    }

    /// <summary>Gets or sets the maximum rating (default 10).</summary>
    public int Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, Math.Max(1, value));
    }

    /// <summary>Gets the computed dot states used by the template.</summary>
    public IReadOnlyList<DotItem> Dots
    {
        get => _dots;
        private set => SetAndRaise(DotsProperty, ref _dots, value);
    }

    private void RebuildDots()
    {
        int max = Math.Max(1, Maximum);
        int val = Math.Clamp(Value, 0, max);
        var items = new DotItem[max];
        for (int i = 0; i < max; i++)
            items[i] = new DotItem(i < val);
        Dots = items;
    }
}
