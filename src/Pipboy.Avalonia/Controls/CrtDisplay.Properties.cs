using Avalonia;
using Avalonia.Media;

namespace Pipboy.Avalonia;

public partial class CrtDisplay
{
    // ── Scanlines ─────────────────────────────────────────────────────────────────────

    /// <summary>Gets or sets whether horizontal scanlines are rendered.</summary>
    public static readonly StyledProperty<bool> EnableScanlinesProperty =
        AvaloniaProperty.Register<CrtDisplay, bool>(nameof(EnableScanlines), defaultValue: true);

    /// <summary>Gets or sets the colour of the scanlines.</summary>
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

    /// <summary>Gets or sets the probability (0–1) that any given pixel cell contains a noise dot.</summary>
    public static readonly StyledProperty<double> NoiseDensityProperty =
        AvaloniaProperty.Register<CrtDisplay, double>(nameof(NoiseDensity), defaultValue: 0.02);

    /// <summary>Gets or sets the opacity of the noise dots (0–1).</summary>
    public static readonly StyledProperty<double> NoiseOpacityProperty =
        AvaloniaProperty.Register<CrtDisplay, double>(nameof(NoiseOpacity), defaultValue: 0.05);

    /// <summary>Gets or sets the logical pixel size of each noise dot (minimum 1).</summary>
    public static readonly StyledProperty<int> NoisePixelSizeProperty =
        AvaloniaProperty.Register<CrtDisplay, int>(nameof(NoisePixelSize), defaultValue: 1);

    /// <summary>Gets or sets the interval in milliseconds between noise refreshes (minimum 16 ms).</summary>
    public static readonly StyledProperty<int> NoiseRefreshIntervalMsProperty =
        AvaloniaProperty.Register<CrtDisplay, int>(nameof(NoiseRefreshIntervalMs), defaultValue: 50);

    // ── Vignette ──────────────────────────────────────────────────────────────────────

    /// <summary>Gets or sets whether the radial edge-darkening vignette is applied.</summary>
    public static readonly StyledProperty<bool> EnableVignetteProperty =
        AvaloniaProperty.Register<CrtDisplay, bool>(nameof(EnableVignette), defaultValue: true);

    /// <summary>Gets or sets the maximum darkness of the vignette corners (0–1).</summary>
    public static readonly StyledProperty<double> VignetteIntensityProperty =
        AvaloniaProperty.Register<CrtDisplay, double>(nameof(VignetteIntensity), defaultValue: 0.35);

    // ── Flicker ───────────────────────────────────────────────────────────────────────

    /// <summary>Gets or sets whether random per-frame brightness dimming (flicker) is applied.</summary>
    public static readonly StyledProperty<bool> EnableFlickerProperty =
        AvaloniaProperty.Register<CrtDisplay, bool>(nameof(EnableFlicker), defaultValue: false);

    /// <summary>Gets or sets the maximum fraction of brightness that can be randomly removed each frame (0–1).</summary>
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
}
