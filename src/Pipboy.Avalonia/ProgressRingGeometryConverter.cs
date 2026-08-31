using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Pipboy.Avalonia;

public sealed class ProgressRingGeometryConverter : IValueConverter
{
    private const double Center = 50;
    private const double Radius = 42;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var percentage = value is double number ? Math.Clamp(number, 0, 100) : 0;
        if (percentage <= 0)
            return new StreamGeometry();

        // A full 360-degree ArcTo has identical endpoints, so keep the endpoint
        // fractionally short while remaining visually complete.
        var sweep = percentage >= 100 ? 359.999 : percentage * 3.6;
        var start = new Point(Center, Center - Radius);
        var radians = (sweep - 90) * Math.PI / 180;
        var end = new Point(
            Center + Radius * Math.Cos(radians),
            Center + Radius * Math.Sin(radians));

        var geometry = new StreamGeometry();
        using var context = geometry.Open();
        context.BeginFigure(start, false);
        context.ArcTo(
            end,
            new Size(Radius, Radius),
            0,
            sweep > 180,
            SweepDirection.Clockwise);
        context.EndFigure(false);
        return geometry;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
