using System;
using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace Pipboy.Avalonia;

public partial class CrtDisplay
{
    // ── Effect renderers (called from CrtEffectsLayer.Render) ─────────────────────────

    private void DrawScanlines(DrawingContext context, Rect bounds)
    {
        var    pen     = GetScanlinePen();
        double spacing = Math.Max(1.0, ScanlineSpacing);
        double startY  = EnableScanlineAnimation ? -_scanlineOffset : 0.0;

        for (double y = startY; y < bounds.Height; y += spacing)
            context.DrawLine(pen, new Point(0, y), new Point(bounds.Width, y));
    }

    private void DrawNoise(DrawingContext context)
    {
        var dots = _noiseDots;
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
        double beamY = PositiveMod(_sw.Elapsed.TotalSeconds * ScanBeamSpeed, range) - bh;

        if (beamY + bh < 0 || beamY > bounds.Height) return;

        IBrush brush = EnableScanBeamGradient
            ? GetScanBeamGradientBrush()
            : GetScanBeamSolidBrush();

        context.DrawRectangle(brush, null, new Rect(0, beamY, bounds.Width, bh));
    }

    private void DrawVignette(DrawingContext context, Rect bounds)
        => context.DrawRectangle(GetVignetteBrush(), null, bounds);

    private void DrawFlicker(DrawingContext context, Rect bounds)
    {
        EnsureFlickerBrushes();
        if (_flickerBrushes.Length == 0) return;
        context.DrawRectangle(_flickerBrushes[_rng.Next(_flickerBrushes.Length)], null, bounds);
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

    private ImmutableSolidColorBrush GetScanBeamSolidBrush()
    {
        var color = ScanBeamColor;
        if (_scanBeamSolidBrush is null || color != _cachedScanBeamColor)
        {
            _cachedScanBeamColor = color;
            _scanBeamSolidBrush  = new ImmutableSolidColorBrush(color);
            _scanBeamBrush       = null; // keep caches in sync
        }
        return _scanBeamSolidBrush;
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
        byte alpha = (byte)(intensity * 220);
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
        _flickerBrushes = new ImmutableSolidColorBrush[FlickerLevels + 1];
        for (int i = 0; i <= FlickerLevels; i++)
        {
            double a = (double)i / FlickerLevels * intensity;
            _flickerBrushes[i] = new ImmutableSolidColorBrush(
                Color.FromArgb((byte)(a * 255), 0, 0, 0));
        }
    }
}
