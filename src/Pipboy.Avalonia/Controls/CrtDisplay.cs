using System;
using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;

namespace Pipboy.Avalonia;

/// <summary>
/// A <see cref="Panel"/> that layers animated CRT monitor effects on top of its child
/// content using only managed <see cref="DrawingContext"/> APIs — fully WASM-safe and AOT-compatible.
/// </summary>
/// <remarks>
/// Effects are drawn in order over the child (all independently toggleable):
/// <list type="bullet">
///   <item><description><b>Scanlines</b> – horizontal lines that optionally scroll upward.</description></item>
///   <item><description><b>Scan beam</b> – a bright phosphor glow band sweeping from top to bottom.</description></item>
///   <item><description><b>Static noise</b> – sparse random bright pixels refreshed at a configurable rate.</description></item>
///   <item><description><b>Vignette</b> – radial darkening of the screen edges.</description></item>
///   <item><description><b>Flicker</b> – subtle random brightness dimming (disabled by default).</description></item>
/// </list>
/// CRT effects are drawn by an internal overlay <see cref="Control"/> that is always the last
/// item in <see cref="Panel.Children"/> and therefore composites on top of all user content.
/// Two timers drive animation: a 16 ms animation timer for smooth motion effects (scanlines,
/// scan beam, flicker, FPS overlay) and a configurable-rate noise timer for static generation.
/// </remarks>
public partial class CrtDisplay : Panel
{
    // ── Overlay layer ─────────────────────────────────────────────────────────────────
    //
    // Panel renders Children in list order — the last item composites on top.
    // _overlay is always kept as the last child via EnsureOverlayIsLast(), so all
    // effect drawing happens above any user-supplied content.

    private sealed class CrtEffectsLayer : Control
    {
        private readonly CrtDisplay _owner;

        internal CrtEffectsLayer(CrtDisplay owner)
        {
            _owner           = owner;
            IsHitTestVisible = false;
        }

        protected override Size MeasureOverride(Size availableSize) => default;
        protected override Size ArrangeOverride(Size finalSize)     => finalSize;

        public override void Render(DrawingContext context)
        {
            var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
            if (bounds.Width <= 0 || bounds.Height <= 0) return;

            if (_owner.EnableScanlines) _owner.DrawScanlines(context, bounds);
            if (_owner.EnableNoise)     _owner.DrawNoise(context);
            if (_owner.EnableScanBeam)  _owner.DrawScanBeam(context, bounds);
            if (_owner.EnableVignette)  _owner.DrawVignette(context, bounds);
            if (_owner.EnableFlicker)   _owner.DrawFlicker(context, bounds);
            if (_owner.ShowFps)         _owner.DrawFps(context, bounds);
        }
    }

    private readonly CrtEffectsLayer _overlay;

    // ── Animation state ───────────────────────────────────────────────────────────────

    private readonly Random    _rng = new();
    private readonly Stopwatch _sw  = Stopwatch.StartNew();
    private DispatcherTimer?   _animTimer;
    private DispatcherTimer?   _noiseTimer;
    private TimeSpan           _lastAnimTick;
    private double             _scanlineOffset;

    // FPS counter
    private int    _frameCount;
    private double _lastFpsUpdateSec;
    private int    _currentFps;

    // ── Render caches ─────────────────────────────────────────────────────────────────

    private readonly record struct NoiseDot(float X, float Y, byte Brightness);
    private NoiseDot[]                 _noiseDots    = Array.Empty<NoiseDot>();
    private ImmutableSolidColorBrush[] _noiseBrushes = Array.Empty<ImmutableSolidColorBrush>();
    private double                     _cachedNoiseAlpha = -1;

    private Pen?   _scanlinePen;
    private Color  _cachedScanlineColor;
    private double _cachedScanlineOpacity = -1;
    private double _cachedScanlineHeight  = -1;

    private LinearGradientBrush?      _scanBeamBrush;
    private ImmutableSolidColorBrush? _scanBeamSolidBrush;
    private Color                     _cachedScanBeamColor = default;

    // When true the scan-beam colour follows the theme automatically.
    // Set to false as soon as the user explicitly assigns ScanBeamColor.
    private bool _autoScanBeamColor = true;
    private bool _updatingFromTheme = false;

    private RadialGradientBrush? _vignetteBrush;
    private double               _cachedVignetteIntensity = -1;

    private const int                  FlickerLevels = 16;
    private ImmutableSolidColorBrush[] _flickerBrushes        = Array.Empty<ImmutableSolidColorBrush>();
    private double                     _cachedFlickerIntensity = -1;

    private FormattedText? _fpsText;
    private int            _cachedFpsValue = -1;

    // ── Constructor ───────────────────────────────────────────────────────────────────

    public CrtDisplay()
    {
        _overlay = new CrtEffectsLayer(this);
        // Keep _overlay last in Children so it always renders on top of user content.
        Children.CollectionChanged += EnsureOverlayIsLast;
        Children.Add(_overlay);
    }

    // ── Static constructor ────────────────────────────────────────────────────────────

    static CrtDisplay()
    {
        ScanlineColorProperty  .Changed.AddClassHandler<CrtDisplay>((c, _) => c._scanlinePen = null);
        ScanlineHeightProperty .Changed.AddClassHandler<CrtDisplay>((c, _) => c._scanlinePen = null);
        ScanlineOpacityProperty.Changed.AddClassHandler<CrtDisplay>((c, _) => c._scanlinePen = null);

        NoiseOpacityProperty.Changed.AddClassHandler<CrtDisplay>((c, _) => c._cachedNoiseAlpha = -1);

        NoiseDensityProperty  .Changed.AddClassHandler<CrtDisplay>((c, _) => c.RebuildNoise());
        NoisePixelSizeProperty.Changed.AddClassHandler<CrtDisplay>((c, _) => c.RebuildNoise());

        ScanBeamColorProperty.Changed.AddClassHandler<CrtDisplay>((c, _) =>
        {
            c._scanBeamBrush = null;
            c._scanBeamSolidBrush = null;
            // Any explicit assignment (AXAML, binding, code) disables auto-theme tracking.
            if (!c._updatingFromTheme) c._autoScanBeamColor = false;
        });
        EnableScanBeamGradientProperty.Changed.AddClassHandler<CrtDisplay>((c, _) =>
        {
            c._scanBeamBrush = null;
            c._scanBeamSolidBrush = null;
        });

        VignetteIntensityProperty.Changed.AddClassHandler<CrtDisplay>((c, _) => c._vignetteBrush = null);
        FlickerIntensityProperty .Changed.AddClassHandler<CrtDisplay>((c, _) => c._cachedFlickerIntensity = -1);

        EnableScanlineAnimationProperty.Changed.AddClassHandler<CrtDisplay>((c, _) => c.UpdateAnimTimer());
        EnableScanBeamProperty         .Changed.AddClassHandler<CrtDisplay>((c, _) => c.UpdateAnimTimer());
        EnableFlickerProperty          .Changed.AddClassHandler<CrtDisplay>((c, _) => c.UpdateAnimTimer());
        ShowFpsProperty                .Changed.AddClassHandler<CrtDisplay>((c, _) => c.UpdateAnimTimer());
        EnableNoiseProperty            .Changed.AddClassHandler<CrtDisplay>((c, _) => c.UpdateNoiseTimer());
        NoiseRefreshIntervalMsProperty .Changed.AddClassHandler<CrtDisplay>((c, _) => c.UpdateNoiseTimer());
    }

    // ── Keep overlay last in Children ─────────────────────────────────────────────────

    private void EnsureOverlayIsLast(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (Children.Count == 0 || Children[^1] == _overlay) return;

        Children.CollectionChanged -= EnsureOverlayIsLast;
        try
        {
            Children.Remove(_overlay);
            Children.Add(_overlay);
        }
        finally
        {
            Children.CollectionChanged += EnsureOverlayIsLast;
        }
    }

    // ── Property changes → invalidate overlay ─────────────────────────────────────────

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property.OwnerType == typeof(CrtDisplay))
            _overlay.InvalidateVisual();
    }

    // ── Layout — stretch all children (including overlay) to fill the slot ────────────

    protected override Size MeasureOverride(Size availableSize)
    {
        Size contentDesired = default;
        foreach (var child in Children)
        {
            child.Measure(availableSize);
            if (child != _overlay)
            {
                var ds = child.DesiredSize;
                contentDesired = new Size(
                    Math.Max(contentDesired.Width,  ds.Width),
                    Math.Max(contentDesired.Height, ds.Height));
            }
        }
        return contentDesired;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var rect = new Rect(finalSize);
        foreach (var child in Children)
            child.Arrange(rect);
        return finalSize;
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────────────

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _scanlineOffset = 0;
        _lastAnimTick   = _sw.Elapsed;
        UpdateAnimTimer();
        UpdateNoiseTimer();
        RebuildNoise();

        if (_autoScanBeamColor)
        {
            ApplyScanBeamColorFromTheme();
            PipboyThemeManager.Instance.ThemeColorChanged += OnThemeColorChanged;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        StopAnimTimer();
        StopNoiseTimer();
        PipboyThemeManager.Instance.ThemeColorChanged -= OnThemeColorChanged;
    }

    private void OnThemeColorChanged(object? sender, ThemeColorChangedEventArgs e)
    {
        if (!_autoScanBeamColor) return;
        ApplyScanBeamColorFromTheme();
    }

    private void ApplyScanBeamColorFromTheme()
    {
        var p = PipboyThemeManager.Instance.PrimaryColor;
        _updatingFromTheme = true;
        ScanBeamColor = Color.FromArgb(40, p.R, p.G, p.B);
        _updatingFromTheme = false;
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        RebuildNoise();
    }

    // ── Timer management ──────────────────────────────────────────────────────────────

    private bool NeedsAnimTimer => EnableScanlineAnimation || EnableScanBeam || EnableFlicker || ShowFps;

    private void UpdateAnimTimer()
    {
        if (NeedsAnimTimer)
        {
            if (_animTimer is null)
            {
                _animTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
                _animTimer.Tick += OnAnimTick;
                _animTimer.Start();
            }
        }
        else
        {
            StopAnimTimer();
        }
    }

    private void StopAnimTimer()
    {
        if (_animTimer is null) return;
        _animTimer.Stop();
        _animTimer.Tick -= OnAnimTick;
        _animTimer = null;
    }

    private void UpdateNoiseTimer()
    {
        if (EnableNoise)
        {
            int intervalMs = Math.Max(16, NoiseRefreshIntervalMs);
            if (_noiseTimer is null)
            {
                _noiseTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(intervalMs) };
                _noiseTimer.Tick += OnNoiseTick;
                _noiseTimer.Start();
            }
            else
            {
                _noiseTimer.Interval = TimeSpan.FromMilliseconds(intervalMs);
            }
        }
        else
        {
            StopNoiseTimer();
        }
    }

    private void StopNoiseTimer()
    {
        if (_noiseTimer is null) return;
        _noiseTimer.Stop();
        _noiseTimer.Tick -= OnNoiseTick;
        _noiseTimer = null;
    }

    // ── Timer callbacks ───────────────────────────────────────────────────────────────

    private void OnAnimTick(object? sender, EventArgs e)
    {
        var now = _sw.Elapsed;
        double dt = Math.Min((now - _lastAnimTick).TotalSeconds, 0.1);
        _lastAnimTick = now;

        _frameCount++;
        double totalSec = now.TotalSeconds;
        if (totalSec - _lastFpsUpdateSec >= 1.0)
        {
            int fps = _frameCount;
            if (_currentFps != fps) { _currentFps = fps; _fpsText = null; }
            _frameCount       = 0;
            _lastFpsUpdateSec = totalSec;
        }

        if (EnableScanlineAnimation)
        {
            double spacing = Math.Max(1.0, ScanlineSpacing);
            _scanlineOffset = PositiveMod(_scanlineOffset + ScanlineAnimSpeed * dt, spacing);
        }

        _overlay.InvalidateVisual();
    }

    private void OnNoiseTick(object? sender, EventArgs e)
    {
        RebuildNoise();
        _overlay.InvalidateVisual();
    }

    // ── Noise pre-computation ─────────────────────────────────────────────────────────

    private void RebuildNoise()
    {
        double w = Bounds.Width;
        double h = Bounds.Height;

        if (w <= 0 || h <= 0 || !EnableNoise)
        {
            _noiseDots = Array.Empty<NoiseDot>();
            return;
        }

        int    pixel   = Math.Max(1, NoisePixelSize);
        double density = Math.Clamp(NoiseDensity, 0.0, 1.0);
        int    cols    = (int)(w / pixel);
        int    rows    = (int)(h / pixel);

        var buffer = new NoiseDot[Math.Max(1, (int)(cols * rows * density * 1.2) + cols)];
        int count  = 0;

        for (int col = 0; col < cols; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                if (_rng.NextDouble() >= density) continue;
                if (count >= buffer.Length) break;
                buffer[count++] = new NoiseDot(
                    (float)(col * pixel), (float)(row * pixel), (byte)_rng.Next(256));
            }
        }

        if (count == buffer.Length)
        {
            _noiseDots = buffer;
        }
        else
        {
            var trimmed = new NoiseDot[count];
            Array.Copy(buffer, trimmed, count);
            _noiseDots = trimmed;
        }
    }

    // ── Utility ───────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Always-positive modulo. Returns 0 when <paramref name="m"/> is zero or negative.
    /// </summary>
    internal static double PositiveMod(double x, double m)
    {
        if (m <= 0) return 0;
        double r = x % m;
        return r < 0 ? r + m : r;
    }
}
