using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Pipboy.Avalonia.Demo.Converters;

public class BoolToBorderBrushConverter : IValueConverter
{
    public IBrush TrueBrush { get; set; } = Brushes.White;

    public IBrush FalseBrush { get; set; } = Brushes.Transparent;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? TrueBrush : FalseBrush;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
