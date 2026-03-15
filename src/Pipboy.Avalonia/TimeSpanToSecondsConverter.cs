using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Pipboy.Avalonia;

/// <summary>
/// Converts a <see cref="TimeSpan"/> to its total-seconds representation as a
/// <see cref="double"/>, for use with <see cref="Avalonia.Controls.ProgressBar.Value"/>
/// and <see cref="Avalonia.Controls.ProgressBar.Maximum"/>.
/// Singleton — AOT and trim safe.
/// </summary>
public sealed class TimeSpanToSecondsConverter : IValueConverter
{
    /// <summary>The shared singleton instance.</summary>
    public static readonly TimeSpanToSecondsConverter Instance = new();

    private TimeSpanToSecondsConverter() { }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is TimeSpan ts ? ts.TotalSeconds : 0.0;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is double d ? TimeSpan.FromSeconds(d) : TimeSpan.Zero;
}
