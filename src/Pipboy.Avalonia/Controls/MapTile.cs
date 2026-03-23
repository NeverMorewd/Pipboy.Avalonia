using System.Windows.Input;
using Avalonia;
using Avalonia.Media;

namespace Pipboy.Avalonia;

/// <summary>
/// A single named region / tile on a <see cref="PipboyMap"/>.
/// Define the shape via <see cref="Geometry"/> and wire commands for MVVM interaction.
/// </summary>
public class MapTile : AvaloniaObject
{
    // ── Shape ────────────────────────────────────────────────────────────────

    /// <summary>
    /// The polygon / path that defines this tile in world-space coordinates
    /// (X = longitude –180…180, Y = latitude 90…–90).
    /// Use <see cref="StreamGeometry"/> or any <see cref="Geometry"/> subtype.
    /// </summary>
    public static readonly StyledProperty<Geometry?> GeometryProperty =
        AvaloniaProperty.Register<MapTile, Geometry?>(nameof(Geometry));

    public Geometry? Geometry
    {
        get => GetValue(GeometryProperty);
        set => SetValue(GeometryProperty, value);
    }

    // ── Identity ─────────────────────────────────────────────────────────────

    /// <summary>Machine-readable identifier / key.</summary>
    public static readonly StyledProperty<string> NameProperty =
        AvaloniaProperty.Register<MapTile, string>(nameof(Name), defaultValue: string.Empty);

    public string Name
    {
        get => GetValue(NameProperty);
        set => SetValue(NameProperty, value);
    }

    /// <summary>Human-readable label shown as a tooltip and on the tile face.</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<MapTile, string?>(nameof(Label));

    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    // ── State ────────────────────────────────────────────────────────────────

    /// <summary>Whether this tile is currently selected.</summary>
    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<MapTile, bool>(nameof(IsSelected));

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>Whether the tile responds to pointer interactions.</summary>
    public static readonly StyledProperty<bool> IsEnabledProperty =
        AvaloniaProperty.Register<MapTile, bool>(nameof(IsEnabled), defaultValue: true);

    public bool IsEnabled
    {
        get => GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    // ── MVVM Commands ────────────────────────────────────────────────────────

    /// <summary>Invoked when the tile is clicked (single click / tap).</summary>
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<MapTile, ICommand?>(nameof(Command));

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Parameter passed to <see cref="Command"/>.</summary>
    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<MapTile, object?>(nameof(CommandParameter));

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>Invoked when the tile is double-clicked / double-tapped.</summary>
    public static readonly StyledProperty<ICommand?> DoubleClickCommandProperty =
        AvaloniaProperty.Register<MapTile, ICommand?>(nameof(DoubleClickCommand));

    public ICommand? DoubleClickCommand
    {
        get => GetValue(DoubleClickCommandProperty);
        set => SetValue(DoubleClickCommandProperty, value);
    }

    // ── Appearance override ──────────────────────────────────────────────────

    /// <summary>
    /// Optional fill color override for this tile.
    /// When <see langword="null"/> the default theme fill is used.
    /// </summary>
    public static readonly StyledProperty<Color?> FillColorProperty =
        AvaloniaProperty.Register<MapTile, Color?>(nameof(FillColor));

    public Color? FillColor
    {
        get => GetValue(FillColorProperty);
        set => SetValue(FillColorProperty, value);
    }

    /// <summary>Application-defined data associated with this tile.</summary>
    public static readonly StyledProperty<object?> TagProperty =
        AvaloniaProperty.Register<MapTile, object?>(nameof(Tag));

    public object? Tag
    {
        get => GetValue(TagProperty);
        set => SetValue(TagProperty, value);
    }

    // ── Internal runtime state (set by PipboyMap renderer) ──────────────────
    internal bool IsHovered { get; set; }
}
