using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Interactivity;

namespace Pipboy.Avalonia;

/// <summary>
/// A retro Pip-Boy style vector map control.
/// <para>
/// Features: pan (drag), zoom (scroll-wheel + pinch), tile hover/click/double-click,
/// right-click / long-press marker placement, crosshair cursor lines, latitude/longitude
/// grid, scale bar, and full MVVM support via <see cref="ICommand"/> bindings.
/// </para>
/// <example>
/// <code>
/// &lt;pipboy:PipboyMap
///     Tiles="{Binding WorldTiles}"
///     Markers="{Binding Markers}"
///     SelectedTile="{Binding Selected, Mode=TwoWay}"
///     TileClickedCommand="{Binding SelectRegionCommand}"
///     MarkerAddedCommand="{Binding AddMarkerCommand}"
///     ShowCrosshair="True" /&gt;
/// </code>
/// </example>
/// </summary>
public partial class PipboyMap : Control
{
    // ── Transform state ──────────────────────────────────────────────────────

    /// <summary>World-to-screen transform (scale + translation).</summary>
    private Matrix _transform = Matrix.Identity;

    // ── Pointer / pan state ──────────────────────────────────────────────────
    private bool _isPanning;
    private Point _lastPointerPos;
    private MapTile? _hoveredTile;

    /// <summary>Current mouse position in screen space (for crosshair).</summary>
    private Point _crosshairPos;
    private bool _crosshairVisible;

    // ── Long-press / touch ───────────────────────────────────────────────────
    private DispatcherTimer? _longPressTimer;
    private Point _longPressScreenPos;
    private const double LongPressMs = 600;

    // ── Double-click tracking ────────────────────────────────────────────────
    private int _lastClickCount;

    // ── Cached brushes (resolved from theme) ────────────────────────────────
    private IBrush? _bgBrush;
    private IBrush? _surfaceBrush;
    private IBrush? _primaryBrush;
    private IBrush? _primaryDarkBrush;
    private IBrush? _primaryLightBrush;
    private IBrush? _textDimBrush;
    private IBrush? _borderBrush;
    private IBrush? _hoverBrush;
    private IBrush? _selectionBrush;
    private IBrush? _textBrush;

    // ── Static constructor ───────────────────────────────────────────────────
    static PipboyMap()
    {
        AffectsRender<PipboyMap>(
            TilesProperty,
            MarkersProperty,
            ShowCrosshairProperty,
            ShowGridProperty,
            ShowTileLabelsProperty,
            ShowScaleBarProperty,
            ZoomProperty,
            TileFillProperty);

        ZoomProperty.Changed.AddClassHandler<PipboyMap>((x, e) =>
        {
            if (e.NewValue is double z)
                x.OnZoomPropertyChanged(z);
        });
    }

    public PipboyMap()
    {
        ClipToBounds = true;
        Focusable = true;

        // Pinch-to-zoom (touch / trackpad)
        AddHandler(Gestures.PinchEvent, OnPinch);
    }

    // ── Lifecycle ────────────────────────────────────────────────────────────

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ResolveBrushes();
        FitToView();

        // Re-resolve when theme colors change
        PipboyThemeManager.Instance.ThemeColorChanged += (_, _) => ResolveBrushes();
    }

    protected override Size MeasureOverride(Size availableSize) => availableSize;

    // ── Theme brush resolution ────────────────────────────────────────────────

    private void ResolveBrushes()
    {
        _bgBrush         = TryFindBrush("PipboyBackgroundBrush");
        _surfaceBrush    = TryFindBrush("PipboySurfaceBrush");
        _primaryBrush    = TryFindBrush("PipboyPrimaryBrush");
        _primaryDarkBrush = TryFindBrush("PipboyPrimaryDarkBrush");
        _primaryLightBrush = TryFindBrush("PipboyPrimaryLightBrush");
        _textDimBrush    = TryFindBrush("PipboyTextDimBrush");
        _borderBrush     = TryFindBrush("PipboyBorderBrush");
        _hoverBrush      = TryFindBrush("PipboyHoverBrush");
        _selectionBrush  = TryFindBrush("PipboySelectionBrush");
        _textBrush       = TryFindBrush("PipboyTextBrush");
        InvalidateVisual();
    }

    private IBrush? TryFindBrush(string key)
    {
        if (this.TryFindResource(key, null, out var res) && res is IBrush b)
            return b;
        return null;
    }

    // ── Zoom property sync ────────────────────────────────────────────────────

    private void OnZoomPropertyChanged(double newZoom)
    {
        // Keep transform scale in sync with the Zoom property when set externally.
        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        ApplyZoom(newZoom / GetCurrentScale(), center);
    }

    private double GetCurrentScale()
    {
        return _transform.M11; // uniform scale
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Resets the pan/zoom so that all tiles fit inside the current viewport.
    /// </summary>
    public void FitToView()
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        // Use world bounds from tiles, or default to lon/lat world extent
        var (minX, minY, maxX, maxY) = GetWorldBounds();

        double worldW = maxX - minX;
        double worldH = maxY - minY;
        if (worldW <= 0 || worldH <= 0) return;

        double scaleX = Bounds.Width  / worldW;
        double scaleY = Bounds.Height / worldH;
        double scale  = Math.Min(scaleX, scaleY) * 0.92; // 4 % padding each side

        double tx = (Bounds.Width  - worldW * scale) / 2 - minX * scale;
        double ty = (Bounds.Height - worldH * scale) / 2 - minY * scale;

        _transform = Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(tx, ty);
        SetCurrentValue(ZoomProperty, scale);
        InvalidateVisual();
    }

    /// <summary>Zooms in by one step around the viewport center.</summary>
    public void ZoomIn()  => StepZoom(+1.25, new Point(Bounds.Width / 2, Bounds.Height / 2));

    /// <summary>Zooms out by one step around the viewport center.</summary>
    public void ZoomOut() => StepZoom(1 / 1.25, new Point(Bounds.Width / 2, Bounds.Height / 2));

    // ── Transform helpers ─────────────────────────────────────────────────────

    private Point WorldToScreen(Point world)
    {
        return world * _transform;
    }

    private Point ScreenToWorld(Point screen)
    {
        _transform.TryInvert(out var inv);
        return screen * inv;
    }

    private (double minX, double minY, double maxX, double maxY) GetWorldBounds()
    {
        var tiles = Tiles;
        if (tiles == null || tiles.Count == 0)
            return (-180, -90, 180, 90);  // full world lon/lat

        double minX = double.MaxValue, minY = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue;

        foreach (var tile in tiles)
        {
            if (tile.Geometry is null) continue;
            var b = tile.Geometry.Bounds;
            if (b.Width == 0 && b.Height == 0) continue;
            minX = Math.Min(minX, b.Left);
            minY = Math.Min(minY, b.Top);
            maxX = Math.Max(maxX, b.Right);
            maxY = Math.Max(maxY, b.Bottom);
        }

        return minX == double.MaxValue
            ? (-180, -90, 180, 90)
            : (minX, minY, maxX, maxY);
    }

    // ── Zoom / pan math ───────────────────────────────────────────────────────

    private void StepZoom(double factor, Point screenPivot)
    {
        double currentScale = GetCurrentScale();
        double target = Math.Clamp(currentScale * factor, MinZoom, MaxZoom);
        double actualFactor = target / currentScale;
        ApplyZoom(actualFactor, screenPivot);
    }

    private void ApplyZoom(double factor, Point screenPivot)
    {
        double currentScale = GetCurrentScale();
        double newScale = Math.Clamp(currentScale * factor, MinZoom, MaxZoom);
        double actualFactor = newScale / currentScale;

        // Scale around pivot: translate pivot to origin, scale, translate back
        double tx = _transform.M31 + screenPivot.X * (1 - actualFactor);
        double ty = _transform.M32 + screenPivot.Y * (1 - actualFactor);

        _transform = new Matrix(
            _transform.M11 * actualFactor, 0,
            0, _transform.M22 * actualFactor,
            tx, ty);

        SetCurrentValue(ZoomProperty, newScale);
        InvalidateVisual();
    }

    // ── Hit testing ───────────────────────────────────────────────────────────

    private MapTile? HitTestTile(Point worldPos)
    {
        var tiles = Tiles;
        if (tiles == null) return null;
        foreach (var tile in tiles)
        {
            if (!tile.IsEnabled) continue;
            if (tile.Geometry is { } g && g.FillContains(worldPos))
                return tile;
        }
        return null;
    }

    private MapMarker? HitTestMarker(Point screenPos)
    {
        var markers = Markers;
        if (markers == null) return null;
        foreach (var m in markers)
        {
            if (!m.IsVisible) continue;
            var sp = WorldToScreen(m.Position);
            var md = sp - screenPos;
            if (Math.Sqrt(md.X * md.X + md.Y * md.Y) < 14)
                return m;
        }
        return null;
    }

    // ── Input: pointer ────────────────────────────────────────────────────────

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        _crosshairVisible = true;
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _crosshairVisible = false;
        if (_hoveredTile != null)
        {
            _hoveredTile.IsHovered = false;
            _hoveredTile = null;
        }
        InvalidateVisual();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        var pos = e.GetPosition(this);
        _crosshairPos = pos;

        // ── Pan ──────────────────────────────────────────────────────────────
        if (_isPanning && IsPanEnabled)
        {
            var delta = pos - _lastPointerPos;
            _transform = new Matrix(
                _transform.M11, _transform.M12,
                _transform.M21, _transform.M22,
                _transform.M31 + delta.X,
                _transform.M32 + delta.Y);
            InvalidateVisual();
        }

        _lastPointerPos = pos;

        // ── Hover hit-test ───────────────────────────────────────────────────
        var worldPos = ScreenToWorld(pos);
        var newHovered = HitTestTile(worldPos);

        if (newHovered != _hoveredTile)
        {
            if (_hoveredTile != null) _hoveredTile.IsHovered = false;
            _hoveredTile = newHovered;
            if (_hoveredTile != null) _hoveredTile.IsHovered = true;
            ToolTip.SetTip(this, _hoveredTile?.Label ?? _hoveredTile?.Name ?? null);
            InvalidateVisual();
        }
        else if (ShowCrosshair)
        {
            InvalidateVisual();
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();
        var pos = e.GetPosition(this);
        _lastPointerPos = pos;
        var props = e.GetCurrentPoint(this).Properties;

        if (props.IsLeftButtonPressed)
        {
            _lastClickCount = e.ClickCount;
            _isPanning = IsPanEnabled;

            // Start long-press timer for touch / long-hold marker placement
            if (IsMarkerPlacementEnabled)
                StartLongPressTimer(pos);
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        StopLongPressTimer();

        var pos = e.GetPosition(this);
        var wasPanning = _isPanning;
        _isPanning = false;

        // Only count as a click if the pointer didn't move much (not a drag)
        var delta = pos - _lastPointerPos;
        var moved = Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
        if (moved > 6) return;

        var props = e.GetCurrentPoint(this).Properties;

        if (props.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased && !wasPanning)
        {
            HandleLeftClick(pos, _lastClickCount);
        }
        else if (props.PointerUpdateKind == PointerUpdateKind.RightButtonReleased)
        {
            ShowMarkerContextMenu(pos);
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        _isPanning = false;
        StopLongPressTimer();
    }

    private void HandleLeftClick(Point screenPos, int clickCount)
    {
        // Check marker hit first
        var markerHit = HitTestMarker(screenPos);
        if (markerHit is not null) return; // markers absorb clicks

        var worldPos = ScreenToWorld(screenPos);
        var tile = HitTestTile(worldPos);

        if (tile == null) return;

        if (clickCount >= 2)
        {
            // Double-click
            tile.DoubleClickCommand?.Execute(tile.CommandParameter ?? tile);
            TileDoubleClickedCommand?.Execute(tile);
        }
        else
        {
            // Single click — toggle selection
            if (SelectedTile != null && SelectedTile != tile)
                SelectedTile.IsSelected = false;

            tile.IsSelected = !tile.IsSelected;
            SelectedTile = tile.IsSelected ? tile : null;

            tile.Command?.Execute(tile.CommandParameter ?? tile);
            TileClickedCommand?.Execute(tile);
        }

        InvalidateVisual();
    }

    // ── Input: scroll wheel zoom ──────────────────────────────────────────────

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        if (!IsZoomEnabled) return;

        var pivot = e.GetPosition(this);
        double factor = e.Delta.Y > 0 ? 1.15 : 1.0 / 1.15;
        StepZoom(factor, pivot);
        e.Handled = true;
    }

    // ── Input: pinch gesture ──────────────────────────────────────────────────

    private void OnPinch(object? sender, PinchEventArgs e)
    {
        if (!IsZoomEnabled) return;
        // Use viewport center as pivot (finger midpoint is not trivially available)
        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        StepZoom(e.Scale, center);
        e.Handled = true;
    }

    // ── Long press → marker placement ────────────────────────────────────────

    private void StartLongPressTimer(Point pos)
    {
        StopLongPressTimer();
        _longPressScreenPos = pos;
        _longPressTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(LongPressMs)
        };
        _longPressTimer.Tick += OnLongPress;
        _longPressTimer.Start();
    }

    private void StopLongPressTimer()
    {
        if (_longPressTimer is null) return;
        _longPressTimer.Stop();
        _longPressTimer.Tick -= OnLongPress;
        _longPressTimer = null;
    }

    private void OnLongPress(object? sender, EventArgs e)
    {
        StopLongPressTimer();
        _isPanning = false;
        ShowMarkerContextMenu(_longPressScreenPos);
    }

    // ── Context menu: add / manage markers ────────────────────────────────────

    private void ShowMarkerContextMenu(Point screenPos)
    {
        if (!IsMarkerPlacementEnabled) return;

        var worldPos = ScreenToWorld(screenPos);
        var existingMarker = HitTestMarker(screenPos);

        var menu = new ContextMenu();

        if (existingMarker is not null)
        {
            // Options for the tapped marker
            var removeItem = new MenuItem { Header = $"[ REMOVE ] {existingMarker.Label ?? "MARKER"}" };
            removeItem.Click += (_, _) =>
            {
                Markers.Remove(existingMarker);
                MarkerRemovedCommand?.Execute(existingMarker);
                InvalidateVisual();
            };
            menu.Items.Add(removeItem);
        }
        else
        {
            // Separator header
            menu.Items.Add(new MenuItem { Header = "─── ADD MARKER ───", IsEnabled = false });

            foreach (PipboyMapMarkerKind kind in Enum.GetValues<PipboyMapMarkerKind>())
            {
                var k = kind; // capture
                var item = new MenuItem { Header = $"[ {GetMarkerLabel(k)} ]" };
                item.Click += (_, _) => PlaceMarker(worldPos, k);
                menu.Items.Add(item);
            }

            // Marker already on map?  Offer bulk-clear
            if (Markers.Count > 0)
            {
                menu.Items.Add(new Separator());
                var clearItem = new MenuItem { Header = "[ CLEAR ALL MARKERS ]" };
                clearItem.Click += (_, _) =>
                {
                    Markers.Clear();
                    InvalidateVisual();
                };
                menu.Items.Add(clearItem);
            }
        }

        ContextMenu = menu;
        menu.Open(this);
    }

    private void PlaceMarker(Point worldPos, PipboyMapMarkerKind kind)
    {
        var marker = new MapMarker
        {
            Position  = worldPos,
            Kind      = kind,
            IsVisible = true,
        };
        Markers.Add(marker);
        MarkerAddedCommand?.Execute(marker);
        InvalidateVisual();
    }

    private static string GetMarkerLabel(PipboyMapMarkerKind kind) => kind switch
    {
        PipboyMapMarkerKind.Pin     => "PIN",
        PipboyMapMarkerKind.Flag    => "FLAG",
        PipboyMapMarkerKind.Star    => "STAR",
        PipboyMapMarkerKind.Skull   => "SKULL",
        PipboyMapMarkerKind.Diamond => "DIAMOND",
        PipboyMapMarkerKind.Circle  => "CIRCLE",
        PipboyMapMarkerKind.Cross   => "CROSS",
        PipboyMapMarkerKind.Quest   => "QUEST",
        _                           => kind.ToString().ToUpperInvariant(),
    };

    // ── Render ────────────────────────────────────────────────────────────────

    public override void Render(DrawingContext ctx)
    {
        var bounds = new Rect(Bounds.Size);
        if (bounds.Width <= 0 || bounds.Height <= 0) return;

        // ── 1. Background ────────────────────────────────────────────────────
        var bg = _bgBrush ?? new SolidColorBrush(Color.FromRgb(10, 18, 10));
        ctx.FillRectangle(bg, bounds);

        // ── 2. Grid ──────────────────────────────────────────────────────────
        if (ShowGrid)
            DrawGrid(ctx, bounds);

        // ── 3. Tiles ─────────────────────────────────────────────────────────
        DrawTiles(ctx);

        // ── 4. Markers ───────────────────────────────────────────────────────
        DrawMarkers(ctx);

        // ── 5. Tile labels ───────────────────────────────────────────────────
        if (ShowTileLabels)
            DrawTileLabels(ctx);

        // ── 6. Crosshair ─────────────────────────────────────────────────────
        if (ShowCrosshair && _crosshairVisible)
            DrawCrosshair(ctx, bounds);

        // ── 7. Scale bar ─────────────────────────────────────────────────────
        if (ShowScaleBar)
            DrawScaleBar(ctx, bounds);

        // ── 8. Zoom indicator ────────────────────────────────────────────────
        DrawZoomIndicator(ctx, bounds);
    }

    // ── Draw: grid ────────────────────────────────────────────────────────────

    private void DrawGrid(DrawingContext ctx, Rect bounds)
    {
        var pen = new Pen(_borderBrush ?? Brushes.DarkGreen, 0.4)
        {
            DashStyle = new DashStyle([4, 6], 0)
        };

        // Latitude lines every 30°, longitude every 30°
        for (double lat = -90; lat <= 90; lat += 30)
        {
            var a = WorldToScreen(new Point(-180, lat));
            var b = WorldToScreen(new Point(180,  lat));
            if (b.X >= 0 && a.X <= bounds.Width)
                ctx.DrawLine(pen, a, b);
        }
        for (double lon = -180; lon <= 180; lon += 30)
        {
            var a = WorldToScreen(new Point(lon,  90));
            var b = WorldToScreen(new Point(lon, -90));
            if (a.Y <= bounds.Height && b.Y >= 0)
                ctx.DrawLine(pen, a, b);
        }
    }

    // ── Draw: tiles ───────────────────────────────────────────────────────────

    private void DrawTiles(DrawingContext ctx)
    {
        using var push = ctx.PushTransform(_transform);

        var defaultFill   = TileFill ?? _surfaceBrush ?? new SolidColorBrush(Color.FromArgb(180, 20, 45, 20));
        var hoverFill     = _hoverBrush    ?? new SolidColorBrush(Color.FromArgb(180, 40, 90, 40));
        var selectedFill  = _selectionBrush ?? new SolidColorBrush(Color.FromArgb(200, 0, 160, 80));
        var borderPen     = new Pen(_primaryDarkBrush ?? Brushes.Green, 0.6);
        var selectedPen   = new Pen(_primaryBrush ?? Brushes.LimeGreen, 1.2);

        var tiles = Tiles;
        if (tiles == null) return;

        foreach (var tile in tiles)
        {
            if (tile.Geometry is not { } geo) continue;

            IBrush fill;
            if (tile.IsSelected)
                fill = selectedFill;
            else if (tile.IsHovered)
                fill = hoverFill;
            else if (tile.FillColor.HasValue)
                fill = new SolidColorBrush(tile.FillColor.Value);
            else
                fill = defaultFill;

            var pen = tile.IsSelected ? selectedPen : borderPen;
            ctx.DrawGeometry(fill, pen, geo);
        }
    }

    // ── Draw: tile labels ─────────────────────────────────────────────────────

    private void DrawTileLabels(DrawingContext ctx)
    {
        var tiles = Tiles;
        if (tiles == null) return;

        double scale = GetCurrentScale();
        if (scale < 1.2) return; // hide labels when too zoomed out

        var typeface = new Typeface("Consolas,Courier New,monospace");
        var textBrush = _textDimBrush ?? Brushes.DarkGreen;

        foreach (var tile in tiles)
        {
            if (tile.Geometry is not { } geo) continue;
            var label = tile.Label ?? tile.Name;
            if (string.IsNullOrEmpty(label)) continue;

            var center = GetGeometryCentroid(geo);
            var screenCenter = WorldToScreen(center);

            double fontSize = Math.Max(8, Math.Min(13, scale * 4));
            var ft = new FormattedText(
                label,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                tile.IsSelected ? (_primaryBrush ?? textBrush) : textBrush);

            var origin = new Point(
                screenCenter.X - ft.Width  / 2,
                screenCenter.Y - ft.Height / 2);

            ctx.DrawText(ft, origin);
        }
    }

    private static Point GetGeometryCentroid(Geometry geo)
    {
        var b = geo.Bounds;
        return new Point(b.X + b.Width / 2, b.Y + b.Height / 2);
    }

    // ── Draw: markers ─────────────────────────────────────────────────────────

    private void DrawMarkers(DrawingContext ctx)
    {
        var markers = Markers;
        if (markers == null) return;

        foreach (var marker in markers)
        {
            if (!marker.IsVisible) continue;
            var sp = WorldToScreen(marker.Position);
            DrawMarkerIcon(ctx, sp, marker);
        }
    }

    private void DrawMarkerIcon(DrawingContext ctx, Point center, MapMarker marker)
    {
        var color = marker.Color
            ?? ((_primaryBrush as SolidColorBrush)?.Color ?? Color.FromRgb(0, 200, 80));

        var fillBrush   = new SolidColorBrush(color);
        var strokeBrush = new SolidColorBrush(Color.FromRgb(
            (byte)Math.Min(255, color.R + 60),
            (byte)Math.Min(255, color.G + 60),
            (byte)Math.Min(255, color.B + 60)));
        var pen = new Pen(strokeBrush, 1.2);

        const double S = 7; // half-size

        switch (marker.Kind)
        {
            case PipboyMapMarkerKind.Pin:
                DrawPin(ctx, center, S, fillBrush, pen);
                break;

            case PipboyMapMarkerKind.Flag:
                DrawFlag(ctx, center, S, fillBrush, pen);
                break;

            case PipboyMapMarkerKind.Star:
                DrawStar(ctx, center, S, fillBrush, pen);
                break;

            case PipboyMapMarkerKind.Skull:
                DrawSkull(ctx, center, S, fillBrush, pen);
                break;

            case PipboyMapMarkerKind.Diamond:
                DrawDiamond(ctx, center, S, fillBrush, pen);
                break;

            case PipboyMapMarkerKind.Circle:
                ctx.DrawEllipse(fillBrush, pen, center, S, S);
                break;

            case PipboyMapMarkerKind.Cross:
                DrawCrossMarker(ctx, center, S, pen);
                break;

            case PipboyMapMarkerKind.Quest:
                DrawQuest(ctx, center, S, fillBrush, pen);
                break;
        }

        // Optional label below icon
        if (!string.IsNullOrEmpty(marker.Label))
        {
            var ft = new FormattedText(
                marker.Label,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Consolas,Courier New,monospace"),
                9,
                fillBrush);
            ctx.DrawText(ft, new Point(center.X - ft.Width / 2, center.Y + S + 3));
        }
    }

    private static void DrawPin(DrawingContext ctx, Point c, double s, IBrush fill, Pen pen)
    {
        var geo = new StreamGeometry();
        using (var sg = geo.Open())
        {
            sg.BeginFigure(new Point(c.X, c.Y + s * 1.8), true);
            sg.ArcTo(new Point(c.X - s * 0.8, c.Y - s * 0.4), new Size(s, s), 0, false, SweepDirection.CounterClockwise);
            sg.ArcTo(new Point(c.X + s * 0.8, c.Y - s * 0.4), new Size(s, s), 0, false, SweepDirection.CounterClockwise);
            sg.LineTo(new Point(c.X, c.Y + s * 1.8));
            sg.EndFigure(true);
        }
        ctx.DrawGeometry(fill, pen, geo);
        ctx.DrawEllipse(Brushes.Black, null, new Point(c.X, c.Y - s * 0.1), s * 0.35, s * 0.35);
    }

    private static void DrawFlag(DrawingContext ctx, Point c, double s, IBrush fill, Pen pen)
    {
        var pole = new Pen(pen.Brush, 1.5);
        ctx.DrawLine(pole, new Point(c.X, c.Y - s * 1.8), new Point(c.X, c.Y + s));
        var geo = new StreamGeometry();
        using (var sg = geo.Open())
        {
            sg.BeginFigure(new Point(c.X, c.Y - s * 1.8), true);
            sg.LineTo(new Point(c.X + s * 1.4, c.Y - s));
            sg.LineTo(new Point(c.X, c.Y - s * 0.2));
            sg.EndFigure(true);
        }
        ctx.DrawGeometry(fill, pen, geo);
    }

    private static void DrawStar(DrawingContext ctx, Point c, double s, IBrush fill, Pen pen)
    {
        var pts = new Point[10];
        for (int i = 0; i < 10; i++)
        {
            double angle = Math.PI / 2 + i * Math.PI / 5;
            double r = (i % 2 == 0) ? s : s * 0.45;
            pts[i] = new Point(c.X + r * Math.Cos(angle), c.Y - r * Math.Sin(angle));
        }
        var geo = new StreamGeometry();
        using (var sg = geo.Open())
        {
            sg.BeginFigure(pts[0], true);
            for (int i = 1; i < pts.Length; i++) sg.LineTo(pts[i]);
            sg.EndFigure(true);
        }
        ctx.DrawGeometry(fill, pen, geo);
    }

    private static void DrawSkull(DrawingContext ctx, Point c, double s, IBrush fill, Pen pen)
    {
        ctx.DrawEllipse(fill, pen, new Point(c.X, c.Y - s * 0.2), s * 0.9, s * 0.85);
        // eye sockets
        ctx.DrawEllipse(Brushes.Black, null, new Point(c.X - s * 0.3, c.Y - s * 0.2), s * 0.22, s * 0.22);
        ctx.DrawEllipse(Brushes.Black, null, new Point(c.X + s * 0.3, c.Y - s * 0.2), s * 0.22, s * 0.22);
        // jaw
        var jawPen = new Pen(pen.Brush, 1);
        for (int t = 0; t < 3; t++)
            ctx.DrawLine(jawPen,
                new Point(c.X - s * 0.3 + t * s * 0.3, c.Y + s * 0.5),
                new Point(c.X - s * 0.3 + t * s * 0.3, c.Y + s * 0.9));
    }

    private static void DrawDiamond(DrawingContext ctx, Point c, double s, IBrush fill, Pen pen)
    {
        var geo = new StreamGeometry();
        using (var sg = geo.Open())
        {
            sg.BeginFigure(new Point(c.X,       c.Y - s * 1.4), true);
            sg.LineTo(new Point(c.X + s,        c.Y));
            sg.LineTo(new Point(c.X,            c.Y + s * 1.4));
            sg.LineTo(new Point(c.X - s,        c.Y));
            sg.EndFigure(true);
        }
        ctx.DrawGeometry(fill, pen, geo);
    }

    private static void DrawCrossMarker(DrawingContext ctx, Point c, double s, Pen pen)
    {
        var thick = new Pen(pen.Brush, 2.5);
        ctx.DrawLine(thick, new Point(c.X - s, c.Y), new Point(c.X + s, c.Y));
        ctx.DrawLine(thick, new Point(c.X, c.Y - s), new Point(c.X, c.Y + s));
    }

    private static void DrawQuest(DrawingContext ctx, Point c, double s, IBrush fill, Pen pen)
    {
        // Triangle
        var geo = new StreamGeometry();
        using (var sg = geo.Open())
        {
            sg.BeginFigure(new Point(c.X,        c.Y - s * 1.5), true);
            sg.LineTo(new Point(c.X + s * 1.3,   c.Y + s));
            sg.LineTo(new Point(c.X - s * 1.3,   c.Y + s));
            sg.EndFigure(true);
        }
        ctx.DrawGeometry(fill, pen, geo);
        // Exclamation
        var ep = new Pen(Brushes.Black, 1.5);
        ctx.DrawLine(ep, new Point(c.X, c.Y - s * 0.8), new Point(c.X, c.Y + s * 0.2));
        ctx.DrawEllipse(Brushes.Black, null, new Point(c.X, c.Y + s * 0.55), 1.5, 1.5);
    }

    // ── Draw: crosshair ───────────────────────────────────────────────────────

    private void DrawCrosshair(DrawingContext ctx, Rect bounds)
    {
        var pen = new Pen(_primaryBrush ?? Brushes.LimeGreen, 0.8)
        {
            DashStyle = new DashStyle([6, 4], 0)
        };

        // Horizontal line
        ctx.DrawLine(pen,
            new Point(0,             _crosshairPos.Y),
            new Point(bounds.Width,  _crosshairPos.Y));

        // Vertical line
        ctx.DrawLine(pen,
            new Point(_crosshairPos.X, 0),
            new Point(_crosshairPos.X, bounds.Height));

        // Center dot
        ctx.DrawEllipse(_primaryBrush ?? Brushes.LimeGreen, null, _crosshairPos, 2, 2);

        // Coordinate readout
        var worldPos = ScreenToWorld(_crosshairPos);
        var coordText = $"{Math.Abs(worldPos.X):F1}°{(worldPos.X >= 0 ? "E" : "W")}  " +
                        $"{Math.Abs(worldPos.Y):F1}°{(worldPos.Y >= 0 ? "N" : "S")}";

        var ft = new FormattedText(
            coordText,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Consolas,Courier New,monospace"),
            10,
            _primaryBrush ?? Brushes.LimeGreen);

        double tx = _crosshairPos.X + 8;
        double ty = _crosshairPos.Y - ft.Height - 4;
        if (tx + ft.Width > bounds.Width)  tx = _crosshairPos.X - ft.Width - 8;
        if (ty < 2)                         ty = _crosshairPos.Y + 6;

        // Shadow for readability
        var shadow = new FormattedText(coordText,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Consolas,Courier New,monospace"),
            10, _bgBrush ?? Brushes.Black);
        ctx.DrawText(shadow, new Point(tx + 1, ty + 1));
        ctx.DrawText(ft, new Point(tx, ty));
    }

    // ── Draw: scale bar ───────────────────────────────────────────────────────

    private void DrawScaleBar(DrawingContext ctx, Rect bounds)
    {
        const double margin  = 16;
        const double barH    = 5;
        const double barW    = 80;

        double bx = margin;
        double by = bounds.Height - margin - barH - 16;

        // How many degrees does barW pixels represent?
        double scale = GetCurrentScale();
        double degreesPerPixel = 1.0 / scale;
        double barDegrees = barW * degreesPerPixel;

        string label = barDegrees >= 1
            ? $"{barDegrees:F0}°"
            : $"{barDegrees * 60:F0}'";

        var bg = new SolidColorBrush(Color.FromArgb(120, 0, 0, 0));
        ctx.FillRectangle(bg, new Rect(bx - 4, by - 4, barW + 8, barH + 24));

        var barBrush = _primaryBrush ?? Brushes.LimeGreen;
        var barPen   = new Pen(barBrush, 1);

        // Scale bar
        ctx.FillRectangle(barBrush, new Rect(bx, by, barW, barH));

        // Tick marks
        ctx.DrawLine(barPen, new Point(bx,        by),      new Point(bx,        by + barH + 3));
        ctx.DrawLine(barPen, new Point(bx + barW, by),      new Point(bx + barW, by + barH + 3));
        ctx.DrawLine(barPen, new Point(bx + barW / 2, by),  new Point(bx + barW / 2, by + barH + 2));

        var ft = new FormattedText(
            label,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Consolas,Courier New,monospace"),
            9,
            _textDimBrush ?? Brushes.Gray);

        ctx.DrawText(ft, new Point(bx + barW / 2 - ft.Width / 2, by + barH + 6));
    }

    // ── Draw: zoom level ──────────────────────────────────────────────────────

    private void DrawZoomIndicator(DrawingContext ctx, Rect bounds)
    {
        var ft = new FormattedText(
            $"ZOOM x{GetCurrentScale():F2}",
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Consolas,Courier New,monospace"),
            9,
            _textDimBrush ?? Brushes.Gray);

        ctx.DrawText(ft, new Point(bounds.Width - ft.Width - 10, bounds.Height - ft.Height - 8));
    }
}
