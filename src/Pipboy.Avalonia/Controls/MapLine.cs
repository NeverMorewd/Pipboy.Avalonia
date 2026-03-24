using Avalonia;
using Avalonia.Media;

namespace Pipboy.Avalonia;

/// <summary>
/// A line segment drawn on the map between two world-space points.
/// </summary>
public class MapLine : AvaloniaObject
{
    /// <summary>Start point in world (map tile) coordinates.</summary>
    public static readonly StyledProperty<Point> StartProperty =
        AvaloniaProperty.Register<MapLine, Point>(nameof(Start));

    public Point Start
    {
        get => GetValue(StartProperty);
        set => SetValue(StartProperty, value);
    }

    /// <summary>End point in world (map tile) coordinates.</summary>
    public static readonly StyledProperty<Point> EndProperty =
        AvaloniaProperty.Register<MapLine, Point>(nameof(End));

    public Point End
    {
        get => GetValue(EndProperty);
        set => SetValue(EndProperty, value);
    }

    /// <summary>Visual style — solid, dashed, dotted, or animated flowing dashes.</summary>
    public static readonly StyledProperty<PipboyMapLineStyle> StyleProperty =
        AvaloniaProperty.Register<MapLine, PipboyMapLineStyle>(
            nameof(Style), defaultValue: PipboyMapLineStyle.Solid);

    public PipboyMapLineStyle Style
    {
        get => GetValue(StyleProperty);
        set => SetValue(StyleProperty, value);
    }

    /// <summary>
    /// When <see langword="true"/> the line is drawn at 2× the base stroke width.
    /// </summary>
    public static readonly StyledProperty<bool> IsThickProperty =
        AvaloniaProperty.Register<MapLine, bool>(nameof(IsThick), defaultValue: false);

    public bool IsThick
    {
        get => GetValue(IsThickProperty);
        set => SetValue(IsThickProperty, value);
    }

    /// <summary>
    /// Override stroke color.  Leave <see langword="null"/> to inherit the map primary color.
    /// </summary>
    public static readonly StyledProperty<Color?> ColorProperty =
        AvaloniaProperty.Register<MapLine, Color?>(nameof(Color));

    public Color? Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Whether this line is rendered.</summary>
    public static readonly StyledProperty<bool> IsVisibleProperty =
        AvaloniaProperty.Register<MapLine, bool>(nameof(IsVisible), defaultValue: true);

    public bool IsVisible
    {
        get => GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    /// <summary>Optional application-defined payload.</summary>
    public static readonly StyledProperty<object?> TagProperty =
        AvaloniaProperty.Register<MapLine, object?>(nameof(Tag));

    public object? Tag
    {
        get => GetValue(TagProperty);
        set => SetValue(TagProperty, value);
    }
}
