using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Media;

namespace Pipboy.Avalonia;

public partial class PipboyMap
{
    // ── Tiles ────────────────────────────────────────────────────────────────

    /// <summary>The collection of map tiles / regions to display.</summary>
    public static readonly StyledProperty<IList<MapTile>> TilesProperty =
        AvaloniaProperty.Register<PipboyMap, IList<MapTile>>(
            nameof(Tiles), defaultValue: []);

    public IList<MapTile> Tiles
    {
        get => GetValue(TilesProperty);
        set => SetValue(TilesProperty, value);
    }

    // ── Markers ──────────────────────────────────────────────────────────────

    /// <summary>The collection of markers / pins shown on the map.</summary>
    public static readonly StyledProperty<IList<MapMarker>> MarkersProperty =
        AvaloniaProperty.Register<PipboyMap, IList<MapMarker>>(
            nameof(Markers), defaultValue: []);

    public IList<MapMarker> Markers
    {
        get => GetValue(MarkersProperty);
        set => SetValue(MarkersProperty, value);
    }

    // ── Zoom ─────────────────────────────────────────────────────────────────

    /// <summary>Current zoom level.  1.0 = fit-to-view baseline.</summary>
    public static readonly StyledProperty<double> ZoomProperty =
        AvaloniaProperty.Register<PipboyMap, double>(nameof(Zoom), defaultValue: 1.0);

    public double Zoom
    {
        get => GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    /// <summary>Minimum zoom level (default 0.3).</summary>
    public static readonly StyledProperty<double> MinZoomProperty =
        AvaloniaProperty.Register<PipboyMap, double>(nameof(MinZoom), defaultValue: 0.3);

    public double MinZoom
    {
        get => GetValue(MinZoomProperty);
        set => SetValue(MinZoomProperty, value);
    }

    /// <summary>Maximum zoom level (default 10.0).</summary>
    public static readonly StyledProperty<double> MaxZoomProperty =
        AvaloniaProperty.Register<PipboyMap, double>(nameof(MaxZoom), defaultValue: 10.0);

    public double MaxZoom
    {
        get => GetValue(MaxZoomProperty);
        set => SetValue(MaxZoomProperty, value);
    }

    // ── Crosshair ────────────────────────────────────────────────────────────

    /// <summary>
    /// When <see langword="true"/> a pair of dashed perpendicular lines follows
    /// the mouse cursor across the entire map surface.
    /// </summary>
    public static readonly StyledProperty<bool> ShowCrosshairProperty =
        AvaloniaProperty.Register<PipboyMap, bool>(nameof(ShowCrosshair), defaultValue: true);

    public bool ShowCrosshair
    {
        get => GetValue(ShowCrosshairProperty);
        set => SetValue(ShowCrosshairProperty, value);
    }

    // ── Grid ─────────────────────────────────────────────────────────────────

    /// <summary>Show background latitude/longitude grid lines (default true).</summary>
    public static readonly StyledProperty<bool> ShowGridProperty =
        AvaloniaProperty.Register<PipboyMap, bool>(nameof(ShowGrid), defaultValue: true);

    public bool ShowGrid
    {
        get => GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }

    // ── Labels ───────────────────────────────────────────────────────────────

    /// <summary>Show tile name labels at their centroid (default true).</summary>
    public static readonly StyledProperty<bool> ShowTileLabelsProperty =
        AvaloniaProperty.Register<PipboyMap, bool>(nameof(ShowTileLabels), defaultValue: true);

    public bool ShowTileLabels
    {
        get => GetValue(ShowTileLabelsProperty);
        set => SetValue(ShowTileLabelsProperty, value);
    }

    // ── Scale bar ────────────────────────────────────────────────────────────

    /// <summary>Show the scale bar in the bottom-left corner (default true).</summary>
    public static readonly StyledProperty<bool> ShowScaleBarProperty =
        AvaloniaProperty.Register<PipboyMap, bool>(nameof(ShowScaleBar), defaultValue: true);

    public bool ShowScaleBar
    {
        get => GetValue(ShowScaleBarProperty);
        set => SetValue(ShowScaleBarProperty, value);
    }

    // ── Selected tile (read-back) ────────────────────────────────────────────

    private MapTile? _selectedTile;

    /// <summary>The tile most recently selected by the user (read-only).</summary>
    public static readonly DirectProperty<PipboyMap, MapTile?> SelectedTileProperty =
        AvaloniaProperty.RegisterDirect<PipboyMap, MapTile?>(
            nameof(SelectedTile), o => o.SelectedTile, (o, v) => o.SelectedTile = v);

    public MapTile? SelectedTile
    {
        get => _selectedTile;
        set => SetAndRaise(SelectedTileProperty, ref _selectedTile, value);
    }

    // ── MVVM Commands ────────────────────────────────────────────────────────

    /// <summary>
    /// Invoked when a tile is single-clicked.
    /// The command parameter is the <see cref="MapTile"/> that was clicked.
    /// </summary>
    public static readonly StyledProperty<ICommand?> TileClickedCommandProperty =
        AvaloniaProperty.Register<PipboyMap, ICommand?>(nameof(TileClickedCommand));

    public ICommand? TileClickedCommand
    {
        get => GetValue(TileClickedCommandProperty);
        set => SetValue(TileClickedCommandProperty, value);
    }

    /// <summary>
    /// Invoked when a tile is double-clicked.
    /// The command parameter is the <see cref="MapTile"/> that was double-clicked.
    /// </summary>
    public static readonly StyledProperty<ICommand?> TileDoubleClickedCommandProperty =
        AvaloniaProperty.Register<PipboyMap, ICommand?>(nameof(TileDoubleClickedCommand));

    public ICommand? TileDoubleClickedCommand
    {
        get => GetValue(TileDoubleClickedCommandProperty);
        set => SetValue(TileDoubleClickedCommandProperty, value);
    }

    /// <summary>
    /// Invoked when the user adds a marker via right-click or long-press.
    /// The command parameter is the newly created <see cref="MapMarker"/>.
    /// </summary>
    public static readonly StyledProperty<ICommand?> MarkerAddedCommandProperty =
        AvaloniaProperty.Register<PipboyMap, ICommand?>(nameof(MarkerAddedCommand));

    public ICommand? MarkerAddedCommand
    {
        get => GetValue(MarkerAddedCommandProperty);
        set => SetValue(MarkerAddedCommandProperty, value);
    }

    /// <summary>
    /// Invoked when the user removes a marker.
    /// The command parameter is the removed <see cref="MapMarker"/>.
    /// </summary>
    public static readonly StyledProperty<ICommand?> MarkerRemovedCommandProperty =
        AvaloniaProperty.Register<PipboyMap, ICommand?>(nameof(MarkerRemovedCommand));

    public ICommand? MarkerRemovedCommand
    {
        get => GetValue(MarkerRemovedCommandProperty);
        set => SetValue(MarkerRemovedCommandProperty, value);
    }

    /// <summary>Marker kind used when a new marker is placed (default Pin).</summary>
    public static readonly StyledProperty<PipboyMapMarkerKind> DefaultMarkerKindProperty =
        AvaloniaProperty.Register<PipboyMap, PipboyMapMarkerKind>(
            nameof(DefaultMarkerKind), defaultValue: PipboyMapMarkerKind.Pin);

    public PipboyMapMarkerKind DefaultMarkerKind
    {
        get => GetValue(DefaultMarkerKindProperty);
        set => SetValue(DefaultMarkerKindProperty, value);
    }

    // ── Interaction flags ────────────────────────────────────────────────────

    /// <summary>Allow the user to pan the map by dragging (default true).</summary>
    public static readonly StyledProperty<bool> IsPanEnabledProperty =
        AvaloniaProperty.Register<PipboyMap, bool>(nameof(IsPanEnabled), defaultValue: true);

    public bool IsPanEnabled
    {
        get => GetValue(IsPanEnabledProperty);
        set => SetValue(IsPanEnabledProperty, value);
    }

    /// <summary>Allow the user to zoom with the scroll wheel / pinch (default true).</summary>
    public static readonly StyledProperty<bool> IsZoomEnabledProperty =
        AvaloniaProperty.Register<PipboyMap, bool>(nameof(IsZoomEnabled), defaultValue: true);

    public bool IsZoomEnabled
    {
        get => GetValue(IsZoomEnabledProperty);
        set => SetValue(IsZoomEnabledProperty, value);
    }

    /// <summary>
    /// Allow the user to place markers via right-click / long-press (default true).
    /// </summary>
    public static readonly StyledProperty<bool> IsMarkerPlacementEnabledProperty =
        AvaloniaProperty.Register<PipboyMap, bool>(nameof(IsMarkerPlacementEnabled), defaultValue: true);

    public bool IsMarkerPlacementEnabled
    {
        get => GetValue(IsMarkerPlacementEnabledProperty);
        set => SetValue(IsMarkerPlacementEnabledProperty, value);
    }

    // ── Theme color overrides (optional) ────────────────────────────────────

    /// <summary>
    /// Override the tile fill color.  Leave <see langword="null"/> to use the
    /// theme surface color automatically.
    /// </summary>
    public static readonly StyledProperty<IBrush?> TileFillProperty =
        AvaloniaProperty.Register<PipboyMap, IBrush?>(nameof(TileFill));

    public IBrush? TileFill
    {
        get => GetValue(TileFillProperty);
        set => SetValue(TileFillProperty, value);
    }
}
