using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
/// right-click / long-press marker placement, crosshair cursor lines, adaptive grid,
/// magnifier lens, blinking markers, and full MVVM support.
/// </para>
/// </summary>
public partial class PipboyMap : Control
{
    // ── Transform state ──────────────────────────────────────────────────────
    private Matrix _transform = Matrix.Identity;

    // ── Pointer / pan state ──────────────────────────────────────────────────
    private bool _isPanning;
    private Point _lastPointerPos;
    private MapTile? _hoveredTile;
    private Point _crosshairPos;
    private bool _crosshairVisible;

    // ── Long-press / touch ───────────────────────────────────────────────────
    private DispatcherTimer? _longPressTimer;
    private Point _longPressScreenPos;
    private const double LongPressMs = 600;

    // ── Double-click tracking ────────────────────────────────────────────────
    private int _lastClickCount;

    // ── Line drawing state ───────────────────────────────────────────────────
    private bool  _isDrawingLine;
    private Point _lineDrawStart;    // world coords at press
    private Point _lineDrawCurrent;  // world coords at current pointer pos (preview)

    // ── Animation (DashedFlow + Ripple marker) ───────────────────────────────
    private DispatcherTimer? _animTimer;
    private double _flowPhase;    // 0..1 — drives DashedFlow dash offset
    private double _ripplePhase;  // 0..1 — drives Ripple ring radius / opacity

    // ── Drag tracking ────────────────────────────────────────────────────────
    /// <summary>Screen position at pointer-down; used to measure total drag distance.</summary>
    private Point _pressStartPos;
    /// <summary>True once pointer has moved beyond <see cref="DragThreshold"/> since press.</summary>
    private bool _hasDragged;
    private const double DragThreshold = 8; // pixels

    // ── Blink ────────────────────────────────────────────────────────────────
    private bool _blinkVisible = true;
    private DispatcherTimer? _blinkTimer;

    // ── Cached brushes (rebuilt on theme change) ──────────────────────────────
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

    // ── Cached pens (rebuilt in ResolveBrushes) ───────────────────────────────
    private Pen? _gridPen;
    private Pen? _tileBorderPen;
    private Pen? _tileSelectedPen;
    private Pen? _crosshairPen;
    private Pen? _magLensBorderPen;

    // ── Static shared typeface and dash styles ────────────────────────────────
    private static readonly Typeface s_monoTypeface   = new("Consolas,Courier New,monospace");
    private static readonly DashStyle s_gridDash      = new([4, 6], 0);
    private static readonly DashStyle s_crosshairDash = new([6, 4], 0);
    private static readonly DashStyle s_connectorDash = new([2, 3], 0);
    private static readonly DashStyle s_dashedStyle   = new([8, 5], 0);
    private static readonly DashStyle s_dottedStyle   = new([1.5, 4], 0);

    // ── Theme-change subscription handle ─────────────────────────────────────
    private EventHandler<ThemeColorChangedEventArgs>? _themeChangedHandler;

    // ── Static constructor ───────────────────────────────────────────────────
    static PipboyMap()
    {
        AffectsRender<PipboyMap>(
            TilesProperty,
            MarkersProperty,
            LinesProperty,
            ShowCrosshairProperty,
            ShowGridProperty,
            ShowTileLabelsProperty,
            ShowScaleBarProperty,
            ShowMagnifierProperty,
            ZoomProperty,
            TileFillProperty,
            InteractionModeProperty);

        ZoomProperty.Changed.AddClassHandler<PipboyMap>((x, e) =>
        {
            if (e.NewValue is double z)
                x.OnZoomPropertyChanged(z);
        });

        MarkersBlinkEnabledProperty.Changed.AddClassHandler<PipboyMap>((x, e) =>
        {
            if (e.NewValue is false)
                x.StopBlinkTimer();     // stop and reset _blinkVisible = true
            else
                x.EnsureBlinkTimer();   // restart if any marker needs blinking
        });

        // Re-subscribe to INotifyCollectionChanged when the collection instance changes
        TilesProperty.Changed.AddClassHandler<PipboyMap>((x, e)   => x.OnManagedCollectionPropertyChanged(e));
        MarkersProperty.Changed.AddClassHandler<PipboyMap>((x, e) => x.OnManagedCollectionPropertyChanged(e));
        LinesProperty.Changed.AddClassHandler<PipboyMap>((x, e)   => x.OnManagedCollectionPropertyChanged(e));
    }

    public PipboyMap()
    {
        ClipToBounds = true;
        Focusable = true;
        AddHandler(Gestures.PinchEvent, OnPinch);
    }

    // ── Lifecycle ────────────────────────────────────────────────────────────

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ResolveBrushes();
        FitToView();

        // Store the handler so it can be unsubscribed in OnUnloaded (prevents leak)
        _themeChangedHandler = (_, _) => ResolveBrushes();
        PipboyThemeManager.Instance.ThemeColorChanged += _themeChangedHandler!;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        PipboyThemeManager.Instance.ThemeColorChanged -= _themeChangedHandler;
        _themeChangedHandler = null;
        _longPressTimer?.Stop();
        StopBlinkTimer();
        StopAnimTimer();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        // First measure: OnLoaded handles FitToView
        if (e.PreviousSize.Width <= 0 || e.PreviousSize.Height <= 0) return;

        // Keep the world point at the old viewport center pinned to the new viewport center
        double dx = (e.NewSize.Width  - e.PreviousSize.Width)  / 2;
        double dy = (e.NewSize.Height - e.PreviousSize.Height) / 2;

        _transform = new Matrix(
            _transform.M11, _transform.M12,
            _transform.M21, _transform.M22,
            _transform.M31 + dx,
            _transform.M32 + dy);

        InvalidateVisual();
    }

    protected override Size MeasureOverride(Size availableSize) => availableSize;

    // ── Theme brush resolution ────────────────────────────────────────────────

    private void ResolveBrushes()
    {
        _bgBrush           = TryFindBrush("PipboyBackgroundBrush");
        _surfaceBrush      = TryFindBrush("PipboySurfaceBrush");
        _primaryBrush      = TryFindBrush("PipboyPrimaryBrush");
        _primaryDarkBrush  = TryFindBrush("PipboyPrimaryDarkBrush");
        _primaryLightBrush = TryFindBrush("PipboyPrimaryLightBrush");
        _textDimBrush      = TryFindBrush("PipboyTextDimBrush");
        _borderBrush       = TryFindBrush("PipboyBorderBrush");
        _hoverBrush        = TryFindBrush("PipboyHoverBrush");
        _selectionBrush    = TryFindBrush("PipboySelectionBrush");
        _textBrush         = TryFindBrush("PipboyTextBrush");

        // Rebuild pens that depend on the resolved brushes
        _gridPen          = new Pen(_borderBrush  ?? Brushes.DarkGreen,  0.4) { DashStyle = s_gridDash };
        _tileBorderPen    = new Pen(_primaryDarkBrush ?? Brushes.Green,  0.6);
        _tileSelectedPen  = new Pen(_primaryBrush ?? Brushes.LimeGreen,  1.2);
        _crosshairPen     = new Pen(_primaryBrush ?? Brushes.LimeGreen,  0.8) { DashStyle = s_crosshairDash };
        _magLensBorderPen = new Pen(_primaryBrush ?? Brushes.LimeGreen,  1.5);

        InvalidateVisual();
    }

    private IBrush? TryFindBrush(string key)
    {
        if (this.TryFindResource(key, null, out var res) && res is IBrush b) return b;
        return null;
    }

    // ── Zoom property sync ────────────────────────────────────────────────────

    private void OnZoomPropertyChanged(double newZoom)
    {
        if (Bounds.Width <= 0) return; // not yet measured — FitToView will handle it
        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        ApplyZoom(newZoom / GetCurrentScale(), center);
    }

    private double GetCurrentScale() => _transform.M11;

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Resets the pan/zoom so that all tiles fit inside the current viewport.</summary>
    public void FitToView()
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0) return;

        var (minX, minY, maxX, maxY) = GetWorldBounds();
        double worldW = maxX - minX;
        double worldH = maxY - minY;
        if (worldW <= 0 || worldH <= 0) return;

        double scaleX = Bounds.Width  / worldW;
        double scaleY = Bounds.Height / worldH;
        double scale  = Math.Min(scaleX, scaleY) * 0.92;

        double tx = (Bounds.Width  - worldW * scale) / 2 - minX * scale;
        double ty = (Bounds.Height - worldH * scale) / 2 - minY * scale;

        _transform = Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(tx, ty);
        SetCurrentValue(ZoomProperty, scale);
        InvalidateVisual();
    }

    /// <summary>Zooms in by one step around the viewport center.</summary>
    public void ZoomIn()  => StepZoom(1.25,      new Point(Bounds.Width / 2, Bounds.Height / 2));

    /// <summary>Zooms out by one step around the viewport center.</summary>
    public void ZoomOut() => StepZoom(1.0 / 1.25, new Point(Bounds.Width / 2, Bounds.Height / 2));

    // ── Transform helpers ─────────────────────────────────────────────────────

    private Point WorldToScreen(Point world) => world * _transform;

    private Point ScreenToWorld(Point screen)
    {
        if (!_transform.TryInvert(out var inv)) return default;
        return screen * inv;
    }

    private (double minX, double minY, double maxX, double maxY) GetWorldBounds()
    {
        var tiles = Tiles;
        if (tiles == null || tiles.Count == 0)
            return (0, 0, 2000, 857);

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

        return minX == double.MaxValue ? (0, 0, 2000, 857) : (minX, minY, maxX, maxY);
    }

    // ── Zoom / pan math ───────────────────────────────────────────────────────

    private void StepZoom(double factor, Point screenPivot)
    {
        double current = GetCurrentScale();
        double target  = Math.Clamp(current * factor, MinZoom, MaxZoom);
        ApplyZoom(target / current, screenPivot);
    }

    /// <summary>
    /// Zoom around <paramref name="screenPivot"/> — the world point under the pivot
    /// stays at the same screen position after zoom (image-viewer behaviour).
    /// </summary>
    private void ApplyZoom(double factor, Point screenPivot)
    {
        double currentScale = GetCurrentScale();
        double newScale     = Math.Clamp(currentScale * factor, MinZoom, MaxZoom);
        double actual       = newScale / currentScale;

        // tx_new = pivot + (tx_old - pivot) * actual
        double tx = screenPivot.X + (_transform.M31 - screenPivot.X) * actual;
        double ty = screenPivot.Y + (_transform.M32 - screenPivot.Y) * actual;

        _transform = new Matrix(
            _transform.M11 * actual, 0,
            0, _transform.M22 * actual,
            tx, ty);

        SetCurrentValue(ZoomProperty, newScale);
        InvalidateVisual();
    }

    // ── Blink timer ───────────────────────────────────────────────────────────

    private void EnsureLongPressTimer()
    {
        if (_longPressTimer is not null) return;
        _longPressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(LongPressMs) };
        _longPressTimer.Tick += OnLongPress;
    }

    private void EnsureBlinkTimer()
    {
        if (_blinkTimer is not null) return;
        _blinkTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _blinkTimer.Tick += (_, _) =>
        {
            _blinkVisible = !_blinkVisible;
            InvalidateVisual();
        };
        _blinkTimer.Start();
    }

    private void StopBlinkTimer()
    {
        _blinkTimer?.Stop();
        _blinkTimer = null;
        _blinkVisible = true;
    }

    // ── Animation timer (DashedFlow + Ripple) ─────────────────────────────────

    private void EnsureAnimTimer()
    {
        if (_animTimer is not null) return;
        _animTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(40) };
        _animTimer.Tick += (_, _) =>
        {
            _flowPhase   = (_flowPhase   + 0.05) % 1.0;
            _ripplePhase = (_ripplePhase + 0.025) % 1.0;
            InvalidateVisual();
        };
        _animTimer.Start();
    }

    private void StopAnimTimer()
    {
        _animTimer?.Stop();
        _animTimer = null;
        _flowPhase = _ripplePhase = 0;
    }

    // ── Collection change subscriptions ───────────────────────────────────────

    /// <summary>
    /// Called by the class-level property-changed handlers for Tiles/Markers/Lines.
    /// Unsubscribes from the old collection's change notifications and subscribes to the new one.
    /// </summary>
    private void OnManagedCollectionPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is INotifyCollectionChanged oldCol)
            oldCol.CollectionChanged -= OnCollectionChanged;
        if (e.NewValue is INotifyCollectionChanged newCol)
            newCol.CollectionChanged += OnCollectionChanged;
        // No explicit InvalidateVisual here — AffectsRender already handles it
    }

    /// <summary>Invalidates visual whenever items are added, removed, or the collection is reset.</summary>
    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => InvalidateVisual();

    // ── Hit testing ───────────────────────────────────────────────────────────

    private MapTile? HitTestTile(Point worldPos)
    {
        var tiles = Tiles;
        if (tiles == null) return null;
        foreach (var tile in tiles)
        {
            if (!tile.IsEnabled) continue;
            if (tile.Geometry is { } g && g.FillContains(worldPos)) return tile;
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
            if (Math.Sqrt(md.X * md.X + md.Y * md.Y) < 14) return m;
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
        if (_hoveredTile != null) { _hoveredTile = null; InvalidateVisual(); }
        else InvalidateVisual();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        var pos = e.GetPosition(this);
        _crosshairPos = pos;

        // Line-draw preview
        if (_isDrawingLine)
        {
            var d = pos - _pressStartPos;
            if (!_hasDragged && d.X * d.X + d.Y * d.Y > DragThreshold * DragThreshold)
                _hasDragged = true;
            _lineDrawCurrent = ScreenToWorld(pos);
            InvalidateVisual();
            return;
        }

        if (_isPanning && IsPanEnabled)
        {
            // Once the pointer travels beyond the drag threshold, commit to a drag:
            // cancel long-press so the marker menu never fires mid-drag.
            if (!_hasDragged)
            {
                var d = pos - _pressStartPos;
                if (d.X * d.X + d.Y * d.Y > DragThreshold * DragThreshold)
                {
                    _hasDragged = true;
                    _longPressTimer?.Stop();
                }
            }

            var delta = pos - _lastPointerPos;
            _transform = new Matrix(
                _transform.M11, _transform.M12,
                _transform.M21, _transform.M22,
                _transform.M31 + delta.X,
                _transform.M32 + delta.Y);
            _lastPointerPos = pos;
            InvalidateVisual();
            return;
        }

        // Hover hit-test
        var worldPos = ScreenToWorld(pos);
        var hit = HitTestTile(worldPos);

        if (hit != _hoveredTile)
        {
            // No IsHovered flag on the data model — hover state lives only in _hoveredTile
            _hoveredTile = hit;
            InvalidateVisual();
        }
        else
        {
            InvalidateVisual(); // crosshair / magnifier update
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();
        var pos = e.GetPosition(this);
        var props = e.GetCurrentPoint(this).Properties;

        if (props.IsLeftButtonPressed)
        {
            _pressStartPos = pos;
            _hasDragged    = false;

            if (InteractionMode == PipboyMapInteractionMode.DrawLine)
            {
                // Line-draw mode: capture the start world point; no panning, no long-press
                _isDrawingLine   = true;
                _lineDrawStart   = ScreenToWorld(pos);
                _lineDrawCurrent = _lineDrawStart;
                EnsureAnimTimer(); // keep preview smooth
                e.Handled = true;
                return;
            }

            // Default Pan mode
            _isPanning      = true;
            _lastPointerPos = pos;
            _lastClickCount = e.ClickCount;

            // Long-press timer (touch / stylus / accessibility) — reuse single instance
            _longPressScreenPos = pos;
            EnsureLongPressTimer();
            _longPressTimer!.Stop();
            _longPressTimer.Start();
        }
        else if (props.IsRightButtonPressed && IsMarkerPlacementEnabled)
        {
            ShowMarkerMenu(pos);
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _longPressTimer?.Stop();

        var pos = e.GetPosition(this);

        // Finalize line drawing
        if (_isDrawingLine)
        {
            _isDrawingLine = false;
            if (_hasDragged) // ignore accidental taps
            {
                var line = new MapLine
                {
                    Start   = _lineDrawStart,
                    End     = ScreenToWorld(pos),
                    Style   = DefaultLineStyle,
                    IsThick = DefaultLineIsThick,
                };
                if (Lines is IList<MapLine> ll) ll.Add(line);
                LineAddedCommand?.Execute(line);
                // DashedFlow and Ripple share the same anim timer
                if (line.Style == PipboyMapLineStyle.DashedFlow)
                    EnsureAnimTimer();
            }
            _hasDragged = false;
            InvalidateVisual();
            return;
        }

        if (_isPanning)
        {
            _isPanning = false;
            bool wasDrag = _hasDragged;
            _hasDragged = false;

            if (!wasDrag)
            {
                var worldPos = ScreenToWorld(pos);

                // Check marker hit first
                var markerHit = HitTestMarker(pos);
                if (markerHit != null)
                {
                    // Remove marker on click
                    if (Markers is IList<MapMarker> ml && ml.Contains(markerHit))
                    {
                        ml.Remove(markerHit);
                        MarkerRemovedCommand?.Execute(markerHit);
                        InvalidateVisual();
                        return;
                    }
                }

                var tileHit = HitTestTile(worldPos);
                if (tileHit != null)
                {
                    if (_lastClickCount >= 2)
                    {
                        tileHit.DoubleClickCommand?.Execute(tileHit.CommandParameter ?? tileHit);
                        TileDoubleClickedCommand?.Execute(tileHit);
                    }
                    else
                    {
                        // Toggle selection
                        if (tileHit.IsSelected)
                        {
                            tileHit.IsSelected = false;
                            if (_selectedTile == tileHit) SelectedTile = null;
                        }
                        else
                        {
                            if (_selectedTile != null) _selectedTile.IsSelected = false;
                            tileHit.IsSelected = true;
                            SelectedTile = tileHit;
                        }
                        TileClickedCommand?.Execute(tileHit);
                        InvalidateVisual();
                    }
                }
            }
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        if (!IsZoomEnabled) return;
        double factor = e.Delta.Y > 0 ? 1.12 : 1.0 / 1.12;
        StepZoom(factor, e.GetPosition(this));
        e.Handled = true;
    }

    private void OnPinch(object? sender, PinchEventArgs e)
    {
        if (!IsZoomEnabled) return;
        StepZoom(e.Scale, new Point(Bounds.Width / 2, Bounds.Height / 2));
    }

    private void OnLongPress(object? sender, EventArgs e)
    {
        _longPressTimer?.Stop();
        if (IsMarkerPlacementEnabled && InteractionMode == PipboyMapInteractionMode.Pan)
            ShowMarkerMenu(_longPressScreenPos);
    }

    // ── Marker placement context menu ─────────────────────────────────────────

    private void ShowMarkerMenu(Point screenPos)
    {
        // Capture screenPos in a local so each menu item closure has the correct position,
        // even if another right-click fires before the first menu is dismissed.
        var capturedPos = screenPos;

        var menu = new ContextMenu();

        foreach (var kind in Enum.GetValues<PipboyMapMarkerKind>())
        {
            var k    = kind;
            var item = new MenuItem { Header = $"[ {MarkerKindLabel(kind)} ]" };
            item.Click += (_, _) => PlaceMarkerWithKind(capturedPos, k);
            menu.Items.Add(item);
        }

        menu.Items.Add(new Separator());
        menu.Items.Add(new MenuItem { Header = "[ CANCEL ]" });

        ContextMenu = menu;
        menu.Open(this);
    }

    private void PlaceMarkerWithKind(Point screenPos, PipboyMapMarkerKind kind)
    {
        var worldPos = ScreenToWorld(screenPos);
        var marker = new MapMarker
        {
            Position   = worldPos,
            Kind       = kind,
            IsBlinking = DefaultMarkerIsBlinking,
        };

        if (Markers is IList<MapMarker> ml) ml.Add(marker);
        MarkerAddedCommand?.Execute(marker);
        EnsureBlinkTimer();
        InvalidateVisual();
    }

    // ── Marker kind label ─────────────────────────────────────────────────────

    private static string MarkerKindLabel(PipboyMapMarkerKind kind) => kind switch
    {
        PipboyMapMarkerKind.Pin     => "PIN",
        PipboyMapMarkerKind.Flag    => "FLAG",
        PipboyMapMarkerKind.Star    => "STAR",
        PipboyMapMarkerKind.Skull   => "SKULL",
        PipboyMapMarkerKind.Diamond => "DIAMOND",
        PipboyMapMarkerKind.Circle  => "CIRCLE",
        PipboyMapMarkerKind.Cross   => "CROSS",
        PipboyMapMarkerKind.Quest   => "QUEST",
        PipboyMapMarkerKind.Ripple  => "RIPPLE",
        _                           => kind.ToString().ToUpperInvariant(),
    };

    // ── Render ────────────────────────────────────────────────────────────────

    public override void Render(DrawingContext ctx)
    {
        var bounds = new Rect(Bounds.Size);
        if (bounds.Width <= 0 || bounds.Height <= 0) return;

        // Blink timer
        bool anyBlink = Markers?.Any(m => m.IsBlinking && m.IsVisible) == true;
        if (anyBlink) EnsureBlinkTimer();
        else if (_blinkTimer is not null) StopBlinkTimer();

        // Animation timer — needed for DashedFlow lines and Ripple markers
        bool needsAnim = _isDrawingLine
            || Lines?.Any(l  => l.IsVisible  && l.Style == PipboyMapLineStyle.DashedFlow) == true
            || Markers?.Any(m => m.IsVisible && m.Kind  == PipboyMapMarkerKind.Ripple)    == true;
        if (needsAnim) EnsureAnimTimer();

        // 1. Background
        var bg = _bgBrush ?? new SolidColorBrush(Color.FromRgb(10, 18, 10));
        ctx.FillRectangle(bg, bounds);

        // 2. Grid
        if (ShowGrid) DrawGrid(ctx, bounds);

        // 3. Tiles
        DrawTiles(ctx);

        // 4. Lines (between tiles and markers so markers render on top)
        DrawLines(ctx);

        // 5. Markers
        DrawMarkers(ctx);

        // 6. Tile labels
        if (ShowTileLabels) DrawTileLabels(ctx);

        // 7. Crosshair
        if (ShowCrosshair && _crosshairVisible) DrawCrosshair(ctx, bounds);

        // 8. Magnifier (drawn after crosshair so it sits on top)
        if (ShowMagnifier && _crosshairVisible) DrawMagnifier(ctx, bounds);

        // 9. Scale bar
        if (ShowScaleBar) DrawScaleBar(ctx, bounds);

        // 10. Zoom indicator
        DrawZoomIndicator(ctx, bounds);
    }

    // ── Draw: grid ────────────────────────────────────────────────────────────

    private void DrawGrid(DrawingContext ctx, Rect bounds)
    {
        // Convert viewport corners to world space so both the interval calculation
        // and the loop range are based on what is actually visible right now.
        // This means:
        //   • Grid density adapts to zoom (zoomed-in = finer lines).
        //   • Lines always fill the entire control — even when the map is smaller
        //     than the viewport or panned partially off-screen.
        var visTopLeft  = ScreenToWorld(new Point(0,            0));
        var visBotRight = ScreenToWorld(new Point(bounds.Width, bounds.Height));

        double vx1 = Math.Min(visTopLeft.X, visBotRight.X);
        double vx2 = Math.Max(visTopLeft.X, visBotRight.X);
        double vy1 = Math.Min(visTopLeft.Y, visBotRight.Y);
        double vy2 = Math.Max(visTopLeft.Y, visBotRight.Y);

        double visW = vx2 - vx1;
        double visH = vy2 - vy1;

        double rawInterval = Math.Min(visW, visH) / 20.0;
        double interval    = NiceGridInterval(rawInterval);
        if (interval <= 0) return;

        var pen = _gridPen ?? new Pen(_borderBrush ?? Brushes.DarkGreen, 0.4) { DashStyle = s_gridDash };

        // Horizontal lines — span full control width, looped over visible Y range
        for (double y = Math.Floor(vy1 / interval) * interval; y <= vy2; y += interval)
        {
            double screenY = WorldToScreen(new Point(0, y)).Y;
            ctx.DrawLine(pen, new Point(0, screenY), new Point(bounds.Width, screenY));
        }

        // Vertical lines — span full control height, looped over visible X range
        for (double x = Math.Floor(vx1 / interval) * interval; x <= vx2; x += interval)
        {
            double screenX = WorldToScreen(new Point(x, 0)).X;
            ctx.DrawLine(pen, new Point(screenX, 0), new Point(screenX, bounds.Height));
        }
    }

    private static double NiceGridInterval(double raw)
    {
        if (raw <= 0) return 1;
        double mag        = Math.Pow(10, Math.Floor(Math.Log10(raw)));
        double normalized = raw / mag;
        double nice       = normalized < 1.5 ? 1 : normalized < 3.5 ? 2 : normalized < 7.5 ? 5 : 10;
        return nice * mag;
    }

    // ── Draw: tiles ───────────────────────────────────────────────────────────

    private void DrawTiles(DrawingContext ctx, Matrix? overrideTransform = null)
    {
        var tiles = Tiles;
        if (tiles == null) return;

        var transform    = overrideTransform ?? _transform;
        var defaultFill  = TileFill ?? _surfaceBrush ?? new SolidColorBrush(Color.FromArgb(180, 20, 45, 20));
        var hoverFill    = _hoverBrush     ?? new SolidColorBrush(Color.FromArgb(180, 40, 90, 40));
        var selectedFill = _selectionBrush ?? new SolidColorBrush(Color.FromArgb(200, 0, 160, 80));
        var borderPen    = _tileBorderPen  ?? new Pen(_primaryDarkBrush ?? Brushes.Green, 0.6);
        var selectedPen  = _tileSelectedPen ?? new Pen(_primaryBrush ?? Brushes.LimeGreen, 1.2);

        using var push = ctx.PushTransform(transform);

        foreach (var tile in tiles)
        {
            if (tile.Geometry is not { } geo) continue;

            // Use reference comparison instead of IsHovered flag on the data model
            IBrush fill = tile.IsSelected        ? selectedFill
                        : tile == _hoveredTile   ? hoverFill
                        : tile.FillColor.HasValue ? new SolidColorBrush(tile.FillColor.Value)
                        : defaultFill;

            ctx.DrawGeometry(fill, tile.IsSelected ? selectedPen : borderPen, geo);
        }
    }

    // ── Draw: tile labels ─────────────────────────────────────────────────────

    private void DrawTileLabels(DrawingContext ctx)
    {
        var tiles = Tiles;
        if (tiles == null) return;

        double scale = GetCurrentScale();
        if (scale < 0.5) return;

        var typeface  = s_monoTypeface;
        var textBrush = _textDimBrush ?? Brushes.DarkGreen;

        foreach (var tile in tiles)
        {
            if (tile.Geometry is not { } geo) continue;
            var label = tile.Label ?? tile.Name;
            if (string.IsNullOrEmpty(label)) continue;

            var center       = GetGeometryCentroid(geo);
            var screenCenter = WorldToScreen(center);

            double fontSize = Math.Max(8, Math.Min(13, scale * 4));
            var ft = new FormattedText(
                label,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                tile.IsSelected ? (_primaryBrush ?? textBrush) : textBrush);

            ctx.DrawText(ft, new Point(screenCenter.X - ft.Width / 2, screenCenter.Y - ft.Height / 2));
        }
    }

    private static Point GetGeometryCentroid(Geometry geo)
    {
        var b = geo.Bounds;
        return new Point(b.X + b.Width / 2, b.Y + b.Height / 2);
    }

    // ── Draw: lines ───────────────────────────────────────────────────────────

    private void DrawLines(DrawingContext ctx, Matrix? overrideTransform = null)
    {
        var transform = overrideTransform ?? _transform;

        var lines = Lines;
        if (lines != null)
        {
            foreach (var line in lines)
            {
                if (!line.IsVisible) continue;

                var sp1 = line.Start * transform;
                var sp2 = line.End   * transform;

                ctx.DrawLine(BuildLinePen(line), sp1, sp2);

                // Endpoint dots
                var dotColor  = line.Color ?? ((_primaryBrush as SolidColorBrush)?.Color ?? Color.FromRgb(0, 200, 80));
                var dotBrush  = new SolidColorBrush(dotColor);
                ctx.DrawEllipse(dotBrush, null, sp1, 3.5, 3.5);
                ctx.DrawEllipse(dotBrush, null, sp2, 3.5, 3.5);
            }
        }

        // Live preview while the user is dragging a new line
        if (_isDrawingLine && _hasDragged)
        {
            var sp1 = _lineDrawStart   * transform;
            var sp2 = _lineDrawCurrent * transform;
            var previewPen = new Pen(
                _primaryBrush ?? Brushes.LimeGreen, 1.5,
                new DashStyle([6, 4], -_flowPhase * 10));
            ctx.DrawLine(previewPen, sp1, sp2);
            ctx.DrawEllipse(_primaryBrush ?? Brushes.LimeGreen, null, sp1, 4, 4);
        }
    }

    private Pen BuildLinePen(MapLine line)
    {
        var color     = line.Color ?? ((_primaryBrush as SolidColorBrush)?.Color ?? Color.FromRgb(0, 200, 80));
        IBrush brush  = new SolidColorBrush(color);
        double width  = line.IsThick ? 3.0 : 1.5;

        DashStyle? dash = line.Style switch
        {
            PipboyMapLineStyle.Solid      => null,
            PipboyMapLineStyle.Dashed     => s_dashedStyle,
            PipboyMapLineStyle.Dotted     => s_dottedStyle,
            PipboyMapLineStyle.DashedFlow => new DashStyle([8, 5], -_flowPhase * 13), // animated — must be per-frame
            _                             => null,
        };

        return dash is null
            ? new Pen(brush, width, lineCap: PenLineCap.Round)
            : new Pen(brush, width, dash,   PenLineCap.Round);
    }

    // ── Draw: markers ─────────────────────────────────────────────────────────

    private void DrawMarkers(DrawingContext ctx, Matrix? overrideTransform = null)
    {
        var markers = Markers;
        if (markers == null) return;

        foreach (var marker in markers)
        {
            if (!marker.IsVisible) continue;
            // Hide during the "off" phase only when the global blink switch is on
            if (MarkersBlinkEnabled && marker.IsBlinking && !_blinkVisible) continue;
            var transform = overrideTransform ?? _transform;
            var sp = marker.Position * transform;
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
        const double S = 7;

        switch (marker.Kind)
        {
            case PipboyMapMarkerKind.Pin:     DrawPin(ctx, center, S, fillBrush, pen);    break;
            case PipboyMapMarkerKind.Flag:    DrawFlag(ctx, center, S, fillBrush, pen);   break;
            case PipboyMapMarkerKind.Star:    DrawStar(ctx, center, S, fillBrush, pen);   break;
            case PipboyMapMarkerKind.Skull:   DrawSkull(ctx, center, S, fillBrush, pen);  break;
            case PipboyMapMarkerKind.Diamond: DrawDiamond(ctx, center, S, fillBrush, pen);break;
            case PipboyMapMarkerKind.Circle:  ctx.DrawEllipse(fillBrush, pen, center, S, S); break;
            case PipboyMapMarkerKind.Cross:   DrawCrossMarker(ctx, center, S, pen);       break;
            case PipboyMapMarkerKind.Quest:   DrawQuest(ctx, center, S, fillBrush, pen);  break;
            case PipboyMapMarkerKind.Ripple:  DrawRipple(ctx, center, S, color);           break;
        }

        if (!string.IsNullOrEmpty(marker.Label))
        {
            var ft = new FormattedText(
                marker.Label,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                s_monoTypeface, 9, fillBrush);
            ctx.DrawText(ft, new Point(center.X - ft.Width / 2, center.Y + S + 3));
        }
    }

    private static void DrawPin(DrawingContext ctx, Point c, double s, IBrush fill, Pen pen)
    {
        // Classic map-pin: round head (top) + triangular needle (bottom)
        double r    = s * 0.95;
        var    head = new Point(c.X, c.Y - s * 0.45);   // circle centre
        var    tip  = new Point(c.X, c.Y + s * 1.6);    // needle tip

        // Needle: triangle whose base straddles the bottom of the head circle
        double neckY    = head.Y + r * 0.72;
        double neckHalf = r * 0.58;
        var needle = new StreamGeometry();
        using (var sg = needle.Open())
        {
            sg.BeginFigure(tip, true);
            sg.LineTo(new Point(c.X - neckHalf, neckY));
            sg.LineTo(new Point(c.X + neckHalf, neckY));
            sg.EndFigure(true);
        }
        ctx.DrawGeometry(fill, pen, needle);

        // Head circle drawn after needle so it cleanly covers the neck seam
        ctx.DrawEllipse(fill, pen, head, r, r);

        // Small inner shadow dot — offset up-left for a subtle 3-D depth effect
        ctx.DrawEllipse(
            new SolidColorBrush(Color.FromArgb(150, 0, 0, 0)),
            null,
            new Point(head.X - r * 0.18, head.Y - r * 0.12),
            r * 0.36, r * 0.36);
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
        ctx.DrawEllipse(Brushes.Black, null, new Point(c.X - s * 0.3, c.Y - s * 0.2), s * 0.22, s * 0.22);
        ctx.DrawEllipse(Brushes.Black, null, new Point(c.X + s * 0.3, c.Y - s * 0.2), s * 0.22, s * 0.22);
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
            sg.BeginFigure(new Point(c.X,     c.Y - s * 1.4), true);
            sg.LineTo(new Point(c.X + s,      c.Y));
            sg.LineTo(new Point(c.X,          c.Y + s * 1.4));
            sg.LineTo(new Point(c.X - s,      c.Y));
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
        var geo = new StreamGeometry();
        using (var sg = geo.Open())
        {
            sg.BeginFigure(new Point(c.X,          c.Y - s * 1.5), true);
            sg.LineTo(new Point(c.X + s * 1.3,     c.Y + s));
            sg.LineTo(new Point(c.X - s * 1.3,     c.Y + s));
            sg.EndFigure(true);
        }
        ctx.DrawGeometry(fill, pen, geo);
        var ep = new Pen(Brushes.Black, 1.5);
        ctx.DrawLine(ep, new Point(c.X, c.Y - s * 0.8), new Point(c.X, c.Y + s * 0.2));
        ctx.DrawEllipse(Brushes.Black, null, new Point(c.X, c.Y + s * 0.55), 1.5, 1.5);
    }

    /// <summary>
    /// Animated concentric rings pulsing outward from the centre dot.
    /// Uses <see cref="_ripplePhase"/> (0..1) advanced by the animation timer.
    /// </summary>
    private void DrawRipple(DrawingContext ctx, Point c, double s, Color color)
    {
        // Solid centre dot
        ctx.DrawEllipse(new SolidColorBrush(color), null, c, s * 0.42, s * 0.42);

        // Three rings staggered by 1/3 of the cycle
        for (int i = 0; i < 3; i++)
        {
            double phase  = (_ripplePhase + i / 3.0) % 1.0;
            double radius = s * 0.42 + phase * s * 2.8;
            byte   alpha  = (byte)(210 * (1.0 - phase));
            if (alpha < 6) continue;
            ctx.DrawEllipse(
                null,
                new Pen(new SolidColorBrush(Color.FromArgb(alpha, color.R, color.G, color.B)), 1.5),
                c, radius, radius);
        }
    }

    // ── Draw: crosshair ───────────────────────────────────────────────────────

    private void DrawCrosshair(DrawingContext ctx, Rect bounds)
    {
        var pen = _crosshairPen
            ?? new Pen(_primaryBrush ?? Brushes.LimeGreen, 0.8) { DashStyle = s_crosshairDash };

        ctx.DrawLine(pen, new Point(0, _crosshairPos.Y), new Point(bounds.Width, _crosshairPos.Y));
        ctx.DrawLine(pen, new Point(_crosshairPos.X, 0), new Point(_crosshairPos.X, bounds.Height));
        ctx.DrawEllipse(_primaryBrush ?? Brushes.LimeGreen, null, _crosshairPos, 2, 2);

        // Coordinate readout (world-space X / Y)
        var worldPos  = ScreenToWorld(_crosshairPos);
        var coordText = $"X:{worldPos.X:F0}  Y:{worldPos.Y:F0}";

        var ft = new FormattedText(coordText,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            s_monoTypeface, 10, _primaryBrush ?? Brushes.LimeGreen);

        double tx = _crosshairPos.X + 8;
        double ty = _crosshairPos.Y - ft.Height - 4;
        if (tx + ft.Width > bounds.Width) tx = _crosshairPos.X - ft.Width - 8;
        if (ty < 2)                       ty = _crosshairPos.Y + 6;

        // Background rect for readability (avoids allocating a second FormattedText for shadow)
        var bgBrush = _bgBrush ?? Brushes.Black;
        ctx.FillRectangle(
            new SolidColorBrush(Color.FromArgb(160,
                (bgBrush as SolidColorBrush)?.Color.R ?? 0,
                (bgBrush as SolidColorBrush)?.Color.G ?? 0,
                (bgBrush as SolidColorBrush)?.Color.B ?? 0)),
            new Rect(tx - 2, ty - 1, ft.Width + 4, ft.Height + 2));
        ctx.DrawText(ft, new Point(tx, ty));
    }

    // ── Draw: magnifier ───────────────────────────────────────────────────────

    private void DrawMagnifier(DrawingContext ctx, Rect bounds)
    {
        double radius  = MagnifierRadius;
        double magZoom = MagnifierZoom;
        var    center  = _crosshairPos;

        // Offset the lens so it doesn't hide the cursor area
        var lensCenter = new Point(
            Math.Clamp(center.X + radius * 1.1, radius, bounds.Width  - radius),
            Math.Clamp(center.Y - radius * 1.1, radius, bounds.Height - radius));

        // Compute magnified transform: the world point under the cursor
        // is shown at the centre of the lens
        double scale  = GetCurrentScale();
        double ms     = scale * magZoom;

        // world_under_cursor in screen = _crosshairPos
        // We want: world * magTransform = lensCenter
        var world = ScreenToWorld(center);
        double mtx = lensCenter.X - world.X * ms;
        double mty = lensCenter.Y - world.Y * ms;

        var magTransform = new Matrix(ms, 0, 0, ms, mtx, mty);

        // Clip to circle
        var clipRect = new Rect(lensCenter.X - radius, lensCenter.Y - radius, radius * 2, radius * 2);
        var clipGeo  = new EllipseGeometry(clipRect);

        using (ctx.PushGeometryClip(clipGeo))
        {
            // Background
            var bg = _bgBrush ?? new SolidColorBrush(Color.FromRgb(10, 18, 10));
            ctx.FillRectangle(bg, clipRect);

            // Grid at magnified scale
            if (ShowGrid)
            {
                var (wx1, wy1, wx2, wy2) = GetWorldBounds();
                double rawInterval = Math.Max(wx2 - wx1, wy2 - wy1) / 20.0;
                double interval    = NiceGridInterval(rawInterval);
                var gridPen = _gridPen ?? new Pen(_borderBrush ?? Brushes.DarkGreen, 0.4) { DashStyle = s_gridDash };
                for (double y = Math.Floor(wy1 / interval) * interval; y <= wy2; y += interval)
                    ctx.DrawLine(gridPen, new Point(wx1, y) * magTransform, new Point(wx2, y) * magTransform);
                for (double x = Math.Floor(wx1 / interval) * interval; x <= wx2; x += interval)
                    ctx.DrawLine(gridPen, new Point(x, wy1) * magTransform, new Point(x, wy2) * magTransform);
            }

            DrawTiles(ctx, magTransform);
            DrawLines(ctx, magTransform);
            DrawMarkers(ctx, magTransform);
        }

        // Lens border
        ctx.DrawEllipse(null,
            _magLensBorderPen ?? new Pen(_primaryBrush ?? Brushes.LimeGreen, 1.5),
            lensCenter, radius, radius);

        // Small crosshair dot at lens center
        ctx.DrawEllipse(_primaryBrush ?? Brushes.LimeGreen, null, lensCenter, 2, 2);
        var cp = new Pen(_primaryBrush ?? Brushes.LimeGreen, 0.8);
        ctx.DrawLine(cp, new Point(lensCenter.X - 6, lensCenter.Y), new Point(lensCenter.X + 6, lensCenter.Y));
        ctx.DrawLine(cp, new Point(lensCenter.X, lensCenter.Y - 6), new Point(lensCenter.X, lensCenter.Y + 6));

        // Connector line from cursor to lens
        var connPen = new Pen(_primaryBrush ?? Brushes.LimeGreen, 0.5) { DashStyle = s_connectorDash };
        ctx.DrawLine(connPen, center, lensCenter);
    }

    // ── Draw: scale bar ───────────────────────────────────────────────────────

    private void DrawScaleBar(DrawingContext ctx, Rect bounds)
    {
        const double margin = 16;
        const double barH   = 5;
        const double barW   = 80;

        double bx = margin;
        double by = bounds.Height - margin - barH - 16;

        double scale = GetCurrentScale();
        double unitsPerPixel = 1.0 / scale;
        double barUnits = barW * unitsPerPixel;

        string label = barUnits >= 100  ? $"{barUnits:F0}u"
                     : barUnits >= 1    ? $"{barUnits:F1}u"
                     : $"{barUnits:F3}u";

        var bg = new SolidColorBrush(Color.FromArgb(120, 0, 0, 0));
        ctx.FillRectangle(bg, new Rect(bx - 4, by - 4, barW + 8, barH + 24));

        var barBrush = _primaryBrush ?? Brushes.LimeGreen;
        var barPen   = new Pen(barBrush, 1);

        ctx.FillRectangle(barBrush, new Rect(bx, by, barW, barH));
        ctx.DrawLine(barPen, new Point(bx,           by), new Point(bx,           by + barH + 3));
        ctx.DrawLine(barPen, new Point(bx + barW,    by), new Point(bx + barW,    by + barH + 3));
        ctx.DrawLine(barPen, new Point(bx + barW/2,  by), new Point(bx + barW/2,  by + barH + 2));

        var ft = new FormattedText(label,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            s_monoTypeface, 9, _textDimBrush ?? Brushes.Gray);

        ctx.DrawText(ft, new Point(bx + barW / 2 - ft.Width / 2, by + barH + 6));
    }

    // ── Draw: zoom level ──────────────────────────────────────────────────────

    private void DrawZoomIndicator(DrawingContext ctx, Rect bounds)
    {
        var ft = new FormattedText($"ZOOM x{GetCurrentScale():F2}",
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            s_monoTypeface, 9, _textDimBrush ?? Brushes.Gray);

        ctx.DrawText(ft, new Point(bounds.Width - ft.Width - 10, bounds.Height - ft.Height - 8));
    }
}
