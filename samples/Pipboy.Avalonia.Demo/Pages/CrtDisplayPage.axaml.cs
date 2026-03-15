using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Pipboy.Avalonia.Demo.Pages;

public partial class CrtDisplayPage : UserControl
{
    public CrtDisplayPage()
    {
        InitializeComponent();
    }

    private void OnPresetClassic(object? sender, RoutedEventArgs e)
    {
        MainCrt.EnableScanlines          = true;
        MainCrt.ScanlineColor            = Colors.Black;
        MainCrt.ScanlineOpacity          = 0.30;
        MainCrt.ScanlineSpacing          = 3.0;
        MainCrt.EnableScanlineAnimation  = true;
        MainCrt.ScanlineAnimSpeed        = 30.0;
        MainCrt.EnableScanBeam           = true;
        MainCrt.ScanBeamHeight           = 40.0;
        MainCrt.EnableNoise              = true;
        MainCrt.NoiseDensity             = 0.02;
        MainCrt.NoiseOpacity             = 0.05;
        MainCrt.EnableVignette           = true;
        MainCrt.VignetteIntensity        = 0.35;
        MainCrt.EnableFlicker            = false;
    }

    private void OnPresetPhosphor(object? sender, RoutedEventArgs e)
    {
        MainCrt.EnableScanlines          = true;
        MainCrt.ScanlineColor            = Color.FromArgb(255, 0, 230, 60);
        MainCrt.ScanlineOpacity          = 0.18;
        MainCrt.ScanlineSpacing          = 3.0;
        MainCrt.EnableScanlineAnimation  = true;
        MainCrt.ScanlineAnimSpeed        = 20.0;
        MainCrt.EnableScanBeam           = true;
        MainCrt.ScanBeamHeight           = 60.0;
        MainCrt.EnableNoise              = false;
        MainCrt.EnableVignette           = true;
        MainCrt.VignetteIntensity        = 0.55;
        MainCrt.EnableFlicker            = false;
    }

    private void OnPresetHeavyStatic(object? sender, RoutedEventArgs e)
    {
        MainCrt.EnableScanlines          = true;
        MainCrt.ScanlineColor            = Colors.Black;
        MainCrt.ScanlineOpacity          = 0.45;
        MainCrt.ScanlineSpacing          = 4.0;
        MainCrt.EnableScanlineAnimation  = false;
        MainCrt.EnableScanBeam           = false;
        MainCrt.EnableNoise              = true;
        MainCrt.NoiseDensity             = 0.06;
        MainCrt.NoiseOpacity             = 0.15;
        MainCrt.NoisePixelSize           = 2;
        MainCrt.EnableVignette           = true;
        MainCrt.VignetteIntensity        = 0.6;
        MainCrt.EnableFlicker            = true;
        MainCrt.FlickerIntensity         = 0.08;
    }

    private void OnPresetMinimal(object? sender, RoutedEventArgs e)
    {
        MainCrt.EnableScanlines          = true;
        MainCrt.ScanlineColor            = Colors.Black;
        MainCrt.ScanlineOpacity          = 0.12;
        MainCrt.ScanlineSpacing          = 3.0;
        MainCrt.EnableScanlineAnimation  = false;
        MainCrt.EnableScanBeam           = false;
        MainCrt.EnableNoise              = false;
        MainCrt.EnableVignette           = true;
        MainCrt.VignetteIntensity        = 0.2;
        MainCrt.EnableFlicker            = false;
    }
}
