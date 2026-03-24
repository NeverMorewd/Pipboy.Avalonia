using Avalonia;
using Avalonia.Media;

namespace Pipboy.Avalonia;

/// <summary>
/// A single map marker / pin placed at a specific world-space coordinate.
/// All properties are <see cref="StyledProperty"/> instances so they can be
/// data-bound from a ViewModel.
/// </summary>
public class MapMarker : AvaloniaObject
{
    // ── Position ────────────────────────────────────────────────────────────

    /// <summary>
    /// World-space position (X = longitude –180…180, Y = latitude 90…–90).
    /// </summary>
    public static readonly StyledProperty<Point> PositionProperty =
        AvaloniaProperty.Register<MapMarker, Point>(nameof(Position));

    public Point Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    // ── Appearance ──────────────────────────────────────────────────────────

    /// <summary>Icon shape drawn at the marker position.</summary>
    public static readonly StyledProperty<PipboyMapMarkerKind> KindProperty =
        AvaloniaProperty.Register<MapMarker, PipboyMapMarkerKind>(
            nameof(Kind), defaultValue: PipboyMapMarkerKind.Pin);

    public PipboyMapMarkerKind Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>Optional text label displayed below the icon.</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<MapMarker, string?>(nameof(Label));

    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>
    /// Accent color for this marker.  When <see langword="null"/> (default)
    /// the theme primary color is used.
    /// </summary>
    public static readonly StyledProperty<Color?> ColorProperty =
        AvaloniaProperty.Register<MapMarker, Color?>(nameof(Color));

    public Color? Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Whether the marker is rendered.</summary>
    public static readonly StyledProperty<bool> IsVisibleProperty =
        AvaloniaProperty.Register<MapMarker, bool>(nameof(IsVisible), defaultValue: true);

    public bool IsVisible
    {
        get => GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    // ── Blink ────────────────────────────────────────────────────────────────

    /// <summary>
    /// When <see langword="true"/> the marker blinks on and off at 500 ms intervals.
    /// Can be toggled off to freeze the marker in a visible state.
    /// </summary>
    public static readonly StyledProperty<bool> IsBlinkingProperty =
        AvaloniaProperty.Register<MapMarker, bool>(nameof(IsBlinking), defaultValue: false);

    public bool IsBlinking
    {
        get => GetValue(IsBlinkingProperty);
        set => SetValue(IsBlinkingProperty, value);
    }

    // ── Arbitrary payload ───────────────────────────────────────────────────

    /// <summary>Application-defined data associated with this marker.</summary>
    public static readonly StyledProperty<object?> TagProperty =
        AvaloniaProperty.Register<MapMarker, object?>(nameof(Tag));

    public object? Tag
    {
        get => GetValue(TagProperty);
        set => SetValue(TagProperty, value);
    }
}
