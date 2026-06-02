using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Pipboy.Avalonia;

public class SegmentFillConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (values.Count < 4)
            return 0d;

        if (values[0] is not double percentage)
            return 0d;

        if (values[1] is not string segment)
            return 0d;

        if (values[2] is not double width)
            return 0d;

        if (values[3] is not double height)
            return 0d;

        double progress = percentage / 100.0;

        Debug.WriteLine($"progress={progress}");
        Debug.WriteLine($"Top={Math.Clamp(progress / 0.25, 0, 1)}");
        Debug.WriteLine($"Right={(progress - 0.25) / 0.25}");

        return segment switch
        {
            "Top" =>
                Math.Clamp(progress / 0.28, 0, 1) * (width-2),

            "Right" =>
                Math.Clamp((progress - 0.28) / 0.25, 0, 1) * (height-2),

            "Bottom" =>
                Math.Clamp((progress - 0.53) / 0.25, 0, 1) * (width-2),

            "Left" =>
                Math.Clamp((progress - 0.78) / 0.25, 0, 1) * (height-2),

            _ => 0d
        };
    }
}