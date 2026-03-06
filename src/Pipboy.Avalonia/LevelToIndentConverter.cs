using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Pipboy.Avalonia;

/// <summary>
/// Converts a TreeViewItem.Level integer to a pixel width for indentation.
/// </summary>
public sealed class LevelToIndentConverter : IValueConverter
{
    public static readonly LevelToIndentConverter Instance = new();
    private const double IndentWidth = 16.0;

    private LevelToIndentConverter() { }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int level && level > 0)
            return level * IndentWidth;
        return 0.0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
