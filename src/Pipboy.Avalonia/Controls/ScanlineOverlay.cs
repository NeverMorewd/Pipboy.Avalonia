using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Pipboy.Avalonia;

/// <summary>
/// A Decorator that draws repeating horizontal scanlines over its child,
/// simulating a CRT monitor effect. Rendering uses only managed Avalonia
/// DrawingContext APIs — fully safe on WASM and AOT builds.
/// </summary>
public class ScanlineOverlay : Decorator
{
    public static readonly StyledProperty<double> LineSpacingProperty =
        AvaloniaProperty.Register<ScanlineOverlay, double>(nameof(LineSpacing), defaultValue: 4.0);

    public static readonly StyledProperty<double> LineOpacityProperty =
        AvaloniaProperty.Register<ScanlineOverlay, double>(nameof(LineOpacity), defaultValue: 0.08);

    static ScanlineOverlay()
    {
        AffectsRender<ScanlineOverlay>(LineSpacingProperty, LineOpacityProperty);
    }

    /// <summary>Gets or sets the vertical distance between scanlines in pixels (minimum 2).</summary>
    public double LineSpacing
    {
        get => GetValue(LineSpacingProperty);
        set => SetValue(LineSpacingProperty, value);
    }

    /// <summary>Gets or sets the opacity of each scanline (0–1).</summary>
    public double LineOpacity
    {
        get => GetValue(LineOpacityProperty);
        set => SetValue(LineOpacityProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        double spacing = Math.Max(2.0, LineSpacing);
        double opacity = Math.Clamp(LineOpacity, 0.0, 1.0);

        if (opacity <= 0 || Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        var pen = new Pen(new SolidColorBrush(Colors.Black, opacity), 1.0);
        double y = 0;
        while (y < Bounds.Height)
        {
            context.DrawLine(pen, new Point(0, y), new Point(Bounds.Width, y));
            y += spacing;
        }
    }
}
