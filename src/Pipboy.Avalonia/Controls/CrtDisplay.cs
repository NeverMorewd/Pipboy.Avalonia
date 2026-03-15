using System;
using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;

namespace Pipboy.Avalonia;

/// <summary>
/// A <see cref="Decorator"/> that layers animated CRT monitor effects on top of its child
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
/// Two timers drive animation: a 16 ms animation timer for smooth motion effects (scanlines,
/// scan beam, flicker, FPS overlay) and a configurable-rate noise timer for static generation.
/// Both timers start only when their effects are enabled and the control is in the visual tree.
/// </remarks>
public class CrtDisplay : Decorator
{
    // ── Scanlines ─────────────────────────────────────────────────────────────────────

    /// <summary>Gets or sets whether horizontal scanlines are rendered.</summary>
    public static readonly StyledProperty<bool> EnableScanlinesProperty =
        AvaloniaProperty.Register<CrtDisplay, bool>(nameof(EnableScanlines), defaultValue: true);

    /// <summary>Gets or sets the colour of the scanlines.
    /// The Pip-Boy theme automatically overrides this to <c>PipboyPrimaryColor</c>.</summary>
    public static readonly StyledProperty<Color> ScanlineColorProperty =
        AvaloniaProperty.Register<CrtDisplay, Color>(nameof(ScanlineColor),
            defaultValue: Colors.Black);

    /// <summary>Gets or sets the vertical distance between scanlines in logical pixels (minimum 1).</summary>
    public static readonly StyledProperty<double> ScanlineSpacingProperty =
        AvaloniaProperty.Register<CrtDisplay, double>(nameof(ScanlineSpacing), defaultValue: 3.0);

    /// <summary>Gets or sets the pen thickness (height) of each scanline in logical pixels (minimum 0.5).</summary>
    public static readonly StyledProperty<double> ScanlineHeightProperty =
        AvaloniaProperty.Register<CrtDisplay, double>(nameof(ScanlineHeight), defaultValue: 1.0);

    /// <summary>Gets or sets the opacity of the scanlines (0–1).</summary>
    public static readonly StyledProperty<double> ScanlineOpacityProperty =
        AvaloniaProperty.Register<CrtDisplay, double>(nameof(ScanlineOpacity), defaultValue: 0.3);

    /// <summary>Gets or sets whether the scanlines scroll upward (animated CRT refresh sweep).</summary>
    public static readonly StyledProperty<bool> EnableScanlineAnimationProperty =
        AvaloniaProperty.Register<CrtDisplay, bool>(nameof(EnableScanlineAnimation), defaultValue: true);

    /// <summary>Gets or sets the scroll speed of the animated scanlines in logical pixels per second.</summary>
    public static readonly StyledProperty<double> ScanlineAnimSpeedProperty =
        AvaloniaProperty.Register<CrtDisplay, double>(nameof(ScanlineAnimSpeed), defaultValue: 30.0);

    // ── Scan beam ─────────────────────────────────────────────────────────────────────

    /// <summary>Gets or sets whether the phosphor glow scan beam is rendered.</summary>
    public static readonly StyledProperty<bool> EnableScanBeamProperty =
        AvaloniaProperty.Register<CrtDisplay, bool>(nameof(EnableScanBeam), defaultValue: true);

    /// <summary>Gets or sets the colour (including alpha) of the scan beam glow.</summary>
    public static readonly StyledProperty<Color> ScanBeamColorProperty =
        AvaloniaProperty.Register<CrtDisplay, Color>(nameof(ScanBeamColor),
            defaultValue: Color.FromArgb(40, 0, 230, 60));

    /// <summary>Gets or sets the vertical height of the scan beam in logical pixels.</summary>
    public static readonly StyledProperty<double> ScanBeamHeightProperty =
        AvaloniaProperty.Register<CrtDisplay, double>(nameof(ScanBeamHeight), defaultValue: 40.0);

    /// <summary>Gets or sets whether the scan beam uses a soft radial gradient (true) or a flat solid colour.</summary>
    public static readonly StyledProperty<bool> EnableScanBeamGradientProperty =
        AvaloniaProperty.Register<CrtDisplay, bool>(nameof(EnableScanBeamGradient), defaultValue: true);

    /// <summary>Gets or sets how fast the scan beam travels downward in logical pixels per second.</summary>
    public static readonly StyledProperty<double> ScanBeamSpeedProperty =
        AvaloniaProperty.Register<CrtDisplay, double>(nameof(ScanBeamSpeed), defaultValue: 60.0);

    // ── Noise ─────────────────────────────────────────────────────────────────────────

    /// <summary>Gets or sets whether animated static noise is rendered.</summary>
    public static readonly StyledProperty<bool> EnableNoiseProperty =
        AvaloniaProperty.Register<CrtDisplay, bool>(nameof(EnableNoise), defaultValue: true);

    /// <summary>Gets or sets the probability (0–1) that any given pixel cell contains a noise dot.
    /// Higher values produce denser static. Typical range: 0.01–0.05.</summary>
    public static readonly StyledProperty<double> NoiseDensityProperty =
        AvaloniaProperty.Register<CrtDisplay, double>(nameof(NoiseDensity), defaultValue: 0.02);

    /// <summary>Gets or sets the opacity of the noise dots (0–1).</summary>
    public static readonly StyledProperty<double> NoiseOpacityProperty =
        AvaloniaProperty.Register<CrtDisplay, double>(nameof(NoiseOpacity), defaultValue: 0.05);

    /// <summary>Gets or sets the logical pixel size of each noise dot (minimum 1).
    /// Larger values produce a coarser, retro look.</summary>
    public static readonly StyledProperty<int> NoisePixelSizeProperty =
        AvaloniaProperty.Register<CrtDisplay, int>(nameof(NoisePixelSize), defaultValue: 1);

    /// <summary>Gets or sets the interval in milliseconds between noise refreshes.
    /// Lower values produce more rapidly flickering static; minimum clamped to 16 ms.</summary>
    public static readonly StyledProperty<int> NoiseRefreshIntervalMsProperty =
        AvaloniaProperty.Register<CrtDisplay, int>(nameof(NoiseRefreshIntervalMs), defaultValue: 50);

    // ── Vignette ──────────────────────────────────────────────────────────────────────

    /// <summary>Gets or sets whether the radial edge-darkening vignette is applied.</summary>
    public static readonly StyledProperty<bool> EnableVignetteProperty =
        AvaloniaProperty.Register<CrtDisplay, bool>(nameof(EnableVignette), defaultValue: true);

    /// <summary>Gets or sets the maximum darkness of the vignette corners (0–1).
    /// 0 = no vignette; 1 = near-black corners.</summary>
    public static readonly StyledProperty<double> VignetteIntensityProperty =
        AvaloniaProperty.Register<CrtDisplay, double>(nameof(VignetteIntensity), defaultValue: 0.35);

    // ── Flicker ───────────────────────────────────────────────────────────────────────

    /// <summary>Gets or sets whether random per-frame brightness dimming (flicker) is applied.
    /// Disabled by default; enable for a more authentic worn-CRT feel.</summary>
    public static readonly StyledProperty<bool> EnableFlickerProperty =
        AvaloniaProperty.Register<CrtDisplay, bool>(nameof(EnableFlicker), defaultValue: false);

    /// <summary>Gets or sets the maximum fraction of brightness that can be randomly removed
    /// each frame (0–1). Typical value: 0.03–0.08.</summary>
    public static readonly StyledProperty<double> FlickerIntensityProperty =
        AvaloniaProperty.Register<CrtDisplay, double>(nameof(FlickerIntensity), defaultValue: 0.04);

    // ── Debug ─────────────────────────────────────────────────────────────────────────

    /// <summary>Gets or sets whether an FPS counter overlay is shown in the bottom-right corner.</summary>
    public static readonly StyledProperty<bool> ShowFpsProperty =
        AvaloniaProperty.Register<CrtDisplay, bool>(nameof(ShowFps), defaultValue: false);

    // ── CLR wrappers ──────────────────────────────────────────────────────────────────

    /// <inheritdoc cref="EnableScanlinesProperty"/>
    public bool EnableScanlines
    {
        get => GetValue(EnableScanlinesProperty);
        set => SetValue(EnableScanlinesProperty, value);
    }
    /// <inheritdoc cref="ScanlineColorProperty"/>
    public Color ScanlineColor
    {
        get => GetValue(ScanlineColorProperty);
        set => SetValue(ScanlineColorProperty, value);
    }
    /// <inheritdoc cref="ScanlineSpacingProperty"/>
    public double ScanlineSpacing
    {
        get => GetValue(ScanlineSpacingProperty);
        set => SetValue(ScanlineSpacingProperty, value);
    }
    /// <inheritdoc cref="ScanlineHeightProperty"/>
    public double ScanlineHeight
    {
        get => GetValue(ScanlineHeightProperty);
        set => SetValue(ScanlineHeightProperty, value);
    }
    /// <inheritdoc cref="ScanlineOpacityProperty"/>
    public double ScanlineOpacity
    {
        get => GetValue(ScanlineOpacityProperty);
        set => SetValue(ScanlineOpacityProperty, value);
    }
    /// <inheritdoc cref="EnableScanlineAnimationProperty"/>
    public bool EnableScanlineAnimation
    {
        get => GetValue(EnableScanlineAnimationProperty);
        set => SetValue(EnableScanlineAnimationProperty, value);
    }
    /// <inheritdoc cref="ScanlineAnimSpeedProperty"/>
    public double ScanlineAnimSpeed
    {
        get => GetValue(ScanlineAnimSpeedProperty);
        set => SetValue(ScanlineAnimSpeedProperty, value);
    }
    /// <inheritdoc cref="EnableScanBeamProperty"/>
    public bool EnableScanBeam
    {
        get => GetValue(EnableScanBeamProperty);
        set => SetValue(EnableScanBeamProperty, value);
    }
    /// <inheritdoc cref="ScanBeamColorProperty"/>
    public Color ScanBeamColor
    {
        get => GetValue(ScanBeamColorProperty);
        set => SetValue(ScanBeamColorProperty, value);
    }
    /// <inheritdoc cref="ScanBeamHeightProperty"/>
    public double ScanBeamHeight
    {
        get => GetValue(ScanBeamHeightProperty);
        set => SetValue(ScanBeamHeightProperty, value);
    }
    /// <inheritdoc cref="EnableScanBeamGradientProperty"/>
    public bool EnableScanBeamGradient
    {
        get => GetValue(EnableScanBeamGradientProperty);
        set => SetValue(EnableScanBeamGradientProperty, value);
    }
    /// <inheritdoc cref="ScanBeamSpeedProperty"/>
    public double ScanBeamSpeed
    {
        get => GetValue(ScanBeamSpeedProperty);
        set => SetValue(ScanBeamSpeedProperty, value);
    }
    /// <inheritdoc cref="EnableNoiseProperty"/>
    public bool EnableNoise
    {
        get => GetValue(EnableNoiseProperty);
        set => SetValue(EnableNoiseProperty, value);
    }
    /// <inheritdoc cref="NoiseDensityProperty"/>
    public double NoiseDensity
    {
        get => GetValue(NoiseDensityProperty);
        set => SetValue(NoiseDensityProperty, value);
    }
    /// <inheritdoc cref="NoiseOpacityProperty"/>
    public double NoiseOpacity
    {
        get => GetValue(NoiseOpacityProperty);
        set => SetValue(NoiseOpacityProperty, value);
    }
    /// <inheritdoc cref="NoisePixelSizeProperty"/>
    public int NoisePixelSize
    {
        get => GetValue(NoisePixelSizeProperty);
        set => SetValue(NoisePixelSizeProperty, value);
    }
    /// <inheritdoc cref="NoiseRefreshIntervalMsProperty"/>
    public int NoiseRefreshIntervalMs
    {
        get => GetValue(NoiseRefreshIntervalMsProperty);
        set => SetValue(NoiseRefreshIntervalMsProperty, value);
    }
    /// <inheritdoc cref="EnableVignetteProperty"/>
    public bool EnableVignette
    {
        get => GetValue(EnableVignetteProperty);
        set => SetValue(EnableVignetteProperty, value);
    }
    /// <inheritdoc cref="VignetteIntensityProperty"/>
    public double VignetteIntensity
    {
        get => GetValue(VignetteIntensityProperty);
        set => SetValue(VignetteIntensityProperty, value);
    }
    /// <inheritdoc cref="EnableFlickerProperty"/>
    public bool EnableFlicker
    {
        get => GetValue(EnableFlickerProperty);
        set => SetValue(EnableFlickerProperty, value);
    }
    /// <inheritdoc cref="FlickerIntensityProperty"/>
    public double FlickerIntensity
    {
        get => GetValue(FlickerIntensityProperty);
        set => SetValue(FlickerIntensityProperty, value);
    }
    /// <inheritdoc cref="ShowFpsProperty"/>
    public bool ShowFps
    {
        get => GetValue(ShowFpsProperty);
        set => SetValue(ShowFpsProperty, value);
    }

    // ── Animation state ───────────────────────────────────────────────────────────────

    private readonly Random    _rng = new();
    private readonly Stopwatch _sw  = Stopwatch.StartNew();
    private DispatcherTimer?   _animTimer;
    private DispatcherTimer?   _noiseTimer;
    private TimeSpan           _lastAnimTick;
    private double             _scanlineOffset; // px within [0, ScanlineSpacing)

    // FPS counter
    private int    _frameCount;
    private double _lastFpsUpdateSec;
    private int    _currentFps;

    // ── Render caches ─────────────────────────────────────────────────────────────────

    // Noise
    private readonly record struct NoiseDot(float X, float Y, byte Brightness);
    private NoiseDot[]                 _noiseDots    = Array.Empty<NoiseDot>();
    private ImmutableSolidColorBrush[] _noiseBrushes = Array.Empty<ImmutableSolidColorBrush>();
    private double                     _cachedNoiseAlpha = -1;

    // Scanline pen
    private Pen?   _scanlinePen;
    private Color  _cachedScanlineColor;
    private double _cachedScanlineOpacity = -1;
    private double _cachedScanlineHeight  = -1;

    // Scan beam gradient brush
    private LinearGradientBrush? _scanBeamBrush;
    private Color                _cachedScanBeamColor = default;

    // Vignette brush
    private RadialGradientBrush? _vignetteBrush;
    private double               _cachedVignetteIntensity = -1;

    // Flicker overlay brushes
    private const int                      FlickerLevels = 16;
    private ImmutableSolidColorBrush[]     _flickerBrushes     = Array.Empty<ImmutableSolidColorBrush>();
    private double                         _cachedFlickerIntensity = -1;

    // FPS text
    private FormattedText? _fpsText;
    private int            _cachedFpsValue = -1;

    // ── Static constructor ────────────────────────────────────────────────────────────

    static CrtDisplay()
    {
        AffectsRender<CrtDisplay>(
            EnableScanlinesProperty, ScanlineColorProperty, ScanlineSpacingProperty,
            ScanlineHeightProperty, ScanlineOpacityProperty, EnableScanlineAnimationProperty,
            EnableScanBeamProperty, ScanBeamColorProperty, ScanBeamHeightProperty,
            EnableScanBeamGradientProperty, ScanBeamSpeedProperty,
            EnableNoiseProperty, NoiseDensityProperty, NoiseOpacityProperty, NoisePixelSizeProperty,
            EnableVignetteProperty, VignetteIntensityProperty,
            EnableFlickerProperty, FlickerIntensityProperty,
            ShowFpsProperty);

        // Cache invalidation — scanline pen
        ScanlineColorProperty  .Changed.AddClassHandler<CrtDisplay>((c, _) => c._scanlinePen = null);
        ScanlineHeightProperty .Changed.AddClassHandler<CrtDisplay>((c, _) => c._scanlinePen = null);
        ScanlineOpacityProperty.Changed.AddClassHandler<CrtDisplay>((c, _) => c._scanlinePen = null);

        // Cache invalidation — noise brush array
        NoiseOpacityProperty.Changed.AddClassHandler<CrtDisplay>((c, _) => c._cachedNoiseAlpha = -1);

        // Cache invalidation — noise dots (positional, depend on density/size)
        NoiseDensityProperty .Changed.AddClassHandler<CrtDisplay>((c, _) => c.RebuildNoise());
        NoisePixelSizeProperty.Changed.AddClassHandler<CrtDisplay>((c, _) => c.RebuildNoise());

        // Cache invalidation — scan beam brush
        ScanBeamColorProperty       .Changed.AddClassHandler<CrtDisplay>((c, _) => c._scanBeamBrush = null);
        EnableScanBeamGradientProperty.Changed.AddClassHandler<CrtDisplay>((c, _) => c._scanBeamBrush = null);

        // Cache invalidation — vignette & flicker
        VignetteIntensityProperty.Changed.AddClassHandler<CrtDisplay>((c, _) => c._vignetteBrush = null);
        FlickerIntensityProperty .Changed.AddClassHandler<CrtDisplay>((c, _) => c._cachedFlickerIntensity = -1);

        // Timer management
        EnableScanlineAnimationProperty.Changed.AddClassHandler<CrtDisplay>((c, _) => c.UpdateAnimTimer());
        EnableScanBeamProperty         .Changed.AddClassHandler<CrtDisplay>((c, _) => c.UpdateAnimTimer());
        EnableFlickerProperty          .Changed.AddClassHandler<CrtDisplay>((c, _) => c.UpdateAnimTimer());
        ShowFpsProperty                .Changed.AddClassHandler<CrtDisplay>((c, _) => c.UpdateAnimTimer());
        EnableNoiseProperty            .Changed.AddClassHandler<CrtDisplay>((c, _) => c.UpdateNoiseTimer());
        NoiseRefreshIntervalMsProperty .Changed.AddClassHandler<CrtDisplay>((c, _) => c.UpdateNoiseTimer());
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
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        StopAnimTimer();
        StopNoiseTimer();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        // Noise dots are positioned by pixel grid; must be rebuilt when size changes.
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
        // Cap dt to avoid a huge jump after a debugger break or OS sleep.
        double dt = Math.Min((now - _lastAnimTick).TotalSeconds, 0.1);
        _lastAnimTick = now;

        // FPS counter
        _frameCount++;
        double totalSec = now.TotalSeconds;
        if (totalSec - _lastFpsUpdateSec >= 1.0)
        {
            int fps = _frameCount;
            if (_currentFps != fps) { _currentFps = fps; _fpsText = null; }
            _frameCount       = 0;
            _lastFpsUpdateSec = totalSec;
        }

        // Advance scanline scroll
        if (EnableScanlineAnimation)
        {
            double spacing = Math.Max(1.0, ScanlineSpacing);
            _scanlineOffset = PositiveMod(_scanlineOffset + ScanlineAnimSpeed * dt, spacing);
        }

        InvalidateVisual();
    }

    private void OnNoiseTick(object? sender, EventArgs e)
    {
        RebuildNoise();
        InvalidateVisual();
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

        // Pre-allocate a worst-case buffer, then trim to actual count.
        var buffer = new NoiseDot[Math.Max(1, (int)(cols * rows * density * 1.2) + cols)];
        int count  = 0;

        for (int col = 0; col < cols; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                if (_rng.NextDouble() >= density) continue;
                if (count >= buffer.Length) break; // safety guard
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

    // ── Render ────────────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        base.Render(context); // draws child first

        var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
        if (bounds.Width <= 0 || bounds.Height <= 0) return;

        if (EnableScanlines) DrawScanlines(context, bounds);
        if (EnableNoise)     DrawNoise(context);
        if (EnableScanBeam)  DrawScanBeam(context, bounds);
        if (EnableVignette)  DrawVignette(context, bounds);
        if (EnableFlicker)   DrawFlicker(context, bounds);
        if (ShowFps)         DrawFps(context, bounds);
    }

    // ── Effect renderers ──────────────────────────────────────────────────────────────

    private void DrawScanlines(DrawingContext context, Rect bounds)
    {
        var    pen     = GetScanlinePen();
        double spacing = Math.Max(1.0, ScanlineSpacing);

        // Start at or just above y=0 so the animated wrap-around is seamless.
        double startY = EnableScanlineAnimation ? -_scanlineOffset : 0.0;

        for (double y = startY; y < bounds.Height; y += spacing)
            context.DrawLine(pen, new Point(0, y), new Point(bounds.Width, y));
    }

    private void DrawNoise(DrawingContext context)
    {
        var dots = _noiseDots; // local ref — safe: both timer and Render run on the UI thread
        if (dots.Length == 0) return;

        EnsureNoiseBrushes();
        int pixel = Math.Max(1, NoisePixelSize);

        foreach (var dot in dots)
            context.DrawRectangle(_noiseBrushes[dot.Brightness], null,
                new Rect(dot.X, dot.Y, pixel, pixel));
    }

    private void DrawScanBeam(DrawingContext context, Rect bounds)
    {
        double bh    = Math.Max(1.0, ScanBeamHeight);
        double range = bounds.Height + bh;
        // Beam position is time-based for smooth frame-rate-independent motion.
        double beamY = PositiveMod(_sw.Elapsed.TotalSeconds * ScanBeamSpeed, range) - bh;

        if (beamY + bh < 0 || beamY > bounds.Height) return;

        IBrush brush = EnableScanBeamGradient
            ? GetScanBeamGradientBrush()
            : new ImmutableSolidColorBrush(ScanBeamColor);

        context.DrawRectangle(brush, null, new Rect(0, beamY, bounds.Width, bh));
    }

    private void DrawVignette(DrawingContext context, Rect bounds)
        => context.DrawRectangle(GetVignetteBrush(), null, bounds);

    private void DrawFlicker(DrawingContext context, Rect bounds)
    {
        EnsureFlickerBrushes();
        if (_flickerBrushes.Length == 0) return;
        // Mostly pick index 0 (transparent) so flicker is subtle rather than constant.
        var brush = _flickerBrushes[_rng.Next(_flickerBrushes.Length)];
        context.DrawRectangle(brush, null, bounds);
    }

    private void DrawFps(DrawingContext context, Rect bounds)
    {
        if (_fpsText is null || _cachedFpsValue != _currentFps)
        {
            _cachedFpsValue = _currentFps;
            _fpsText = new FormattedText(
                $"FPS: {_currentFps}",
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Consolas,Courier New,monospace"),
                             FontStyle.Normal, FontWeight.Bold, FontStretch.Normal),
                13,
                new ImmutableSolidColorBrush(Colors.Lime));
        }

        double x = bounds.Width  - _fpsText.Width  - 6;
        double y = bounds.Height - _fpsText.Height - 6;
        context.DrawRectangle(
            new ImmutableSolidColorBrush(Color.FromArgb(120, 0, 0, 0)), null,
            new Rect(x - 3, y - 2, _fpsText.Width + 6, _fpsText.Height + 4));
        context.DrawText(_fpsText, new Point(x, y));
    }

    // ── Cache helpers ─────────────────────────────────────────────────────────────────

    private Pen GetScanlinePen()
    {
        var    color   = ScanlineColor;
        double opacity = Math.Clamp(ScanlineOpacity, 0.0, 1.0);
        double height  = Math.Max(0.5, ScanlineHeight);

        if (_scanlinePen is null
            || color   != _cachedScanlineColor
            || Math.Abs(opacity - _cachedScanlineOpacity) > 0.001
            || Math.Abs(height  - _cachedScanlineHeight)  > 0.001)
        {
            _cachedScanlineColor   = color;
            _cachedScanlineOpacity = opacity;
            _cachedScanlineHeight  = height;
            _scanlinePen = new Pen(new ImmutableSolidColorBrush(color, opacity), height);
        }

        return _scanlinePen;
    }

    private void EnsureNoiseBrushes()
    {
        double opacity = Math.Clamp(NoiseOpacity, 0.0, 1.0);
        if (Math.Abs(_cachedNoiseAlpha - opacity) <= 0.001) return;

        _cachedNoiseAlpha = opacity;
        byte alpha = (byte)(opacity * 255);
        _noiseBrushes = new ImmutableSolidColorBrush[256];
        for (int i = 0; i < 256; i++)
            _noiseBrushes[i] = new ImmutableSolidColorBrush(
                Color.FromArgb(alpha, (byte)i, (byte)i, (byte)i));
    }

    private LinearGradientBrush GetScanBeamGradientBrush()
    {
        var color = ScanBeamColor;
        if (_scanBeamBrush is null || color != _cachedScanBeamColor)
        {
            _cachedScanBeamColor = color;
            var transparent = Color.FromArgb(0, color.R, color.G, color.B);
            _scanBeamBrush = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0.0, 0.0, RelativeUnit.Relative),
                EndPoint   = new RelativePoint(0.0, 1.0, RelativeUnit.Relative)
            };
            _scanBeamBrush.GradientStops.Add(new GradientStop(transparent, 0.00));
            _scanBeamBrush.GradientStops.Add(new GradientStop(color,       0.30));
            _scanBeamBrush.GradientStops.Add(new GradientStop(color,       0.70));
            _scanBeamBrush.GradientStops.Add(new GradientStop(transparent, 1.00));
        }

        return _scanBeamBrush;
    }

    private RadialGradientBrush GetVignetteBrush()
    {
        double intensity = Math.Clamp(VignetteIntensity, 0.0, 1.0);
        if (_vignetteBrush is not null && Math.Abs(_cachedVignetteIntensity - intensity) <= 0.001)
            return _vignetteBrush;

        _cachedVignetteIntensity = intensity;
        byte alpha = (byte)(intensity * 220); // max ~220 so content stays legible at corners
        var brush = new RadialGradientBrush
        {
            Center         = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
            GradientOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
            RadiusX        = new RelativeScalar(0.75, RelativeUnit.Relative),
            RadiusY        = new RelativeScalar(0.75, RelativeUnit.Relative)
        };
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(0,     0, 0, 0), 0.30));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(alpha, 0, 0, 0), 1.00));
        _vignetteBrush = brush;
        return brush;
    }

    private void EnsureFlickerBrushes()
    {
        double intensity = Math.Clamp(FlickerIntensity, 0.0, 1.0);
        if (Math.Abs(_cachedFlickerIntensity - intensity) <= 0.001 && _flickerBrushes.Length > 0) return;

        _cachedFlickerIntensity = intensity;
        // Index 0 is fully transparent (most frames → no dimming).
        _flickerBrushes = new ImmutableSolidColorBrush[FlickerLevels + 1];
        for (int i = 0; i <= FlickerLevels; i++)
        {
            double alpha = (double)i / FlickerLevels * intensity;
            _flickerBrushes[i] = new ImmutableSolidColorBrush(
                Color.FromArgb((byte)(alpha * 255), 0, 0, 0));
        }
    }

    // ── Utility ───────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Always-positive modulo. Returns 0 when <paramref name="m"/> is zero or negative.
    /// </summary>
    public static double PositiveMod(double x, double m)
    {
        if (m <= 0) return 0;
        double r = x % m;
        return r < 0 ? r + m : r;
    }
}
