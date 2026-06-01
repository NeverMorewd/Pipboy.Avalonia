using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Pipboy.Avalonia;

public class RingFillConverter : IMultiValueConverter
{
    public object Convert(System.Collections.Generic.IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2 || values[0] is not double percentage || values[1] is not string segment)
            return 0.0;

        double progress = percentage / 100.0;
        double totalLength = 400.0;

        switch (segment)
        {
            case "Top":
                return Math.Min(1.0, progress / 0.25) * 100.0;
            case "Right":
                return Math.Max(0.0, Math.Min(1.0, (progress - 0.25) / 0.25)) * 100.0;
            case "Bottom":
                return Math.Max(0.0, Math.Min(1.0, (progress - 0.50) / 0.25)) * 100.0;
            case "Left":
                return Math.Max(0.0, Math.Min(1.0, (progress - 0.75) / 0.25)) * 100.0;
            default:
                return 0.0;
        }
    }
}
