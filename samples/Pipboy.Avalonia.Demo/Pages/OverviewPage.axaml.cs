using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Pipboy.Avalonia;
using System;

namespace Pipboy.Avalonia.Demo.Pages;

public partial class OverviewPage : UserControl
{
    // Maps hex colour to display name — kept in sync with the AXAML swatches.
    private static readonly (string Hex, string Name)[] Presets =
    [
        ("#15FF52", "PIPBOY GREEN"), ("#FFA500", "AMBER"),   ("#00BFFF", "BLUE"),   ("#FF3030", "RED"),
        ("#00FFEE", "CYAN"),         ("#BB44FF", "PURPLE"),  ("#FFD700", "GOLD"),   ("#FF8C00", "ORANGE"),
        ("#607D8B", "SLATE"),        ("#9E9E9E", "GRAPHITE"),("#88C0D0", "NORD"),   ("#0F52BA", "COBALT"),
        ("#5C6BC0", "INDIGO"),       ("#FF4080", "ROSE"),    ("#39FF14", "NEON"),   ("#FF8FAB", "SAKURA"),
    ];

    public OverviewPage()
    {
        InitializeComponent();
        // Reflect the current theme in the label on first load.
        UpdateSwatchLabel(PipboyThemeManager.Instance.PrimaryColor.ToString());
    }

    private void OnCountdownStart(object? sender, RoutedEventArgs e)  => OverviewCountdown.Start();
    private void OnCountdownStop(object? sender, RoutedEventArgs e)   => OverviewCountdown.Stop();
    private void OnCountdownReset(object? sender, RoutedEventArgs e)  => OverviewCountdown.Reset();

    private void OnSwatchPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border { Tag: string hex }) return;
        PipboyThemeManager.Instance.TrySetPrimaryColor(hex);
        UpdateSwatchLabel(hex);
    }

    private void UpdateSwatchLabel(string hex)
    {
        var name = "CUSTOM";
        foreach (var (h, n) in Presets)
            if (string.Equals(h, hex, StringComparison.OrdinalIgnoreCase)) { name = n; break; }
        SwatchLabel.Text = $"{name}  {hex.ToUpperInvariant()}";
    }
}
