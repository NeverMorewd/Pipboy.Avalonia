using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Pipboy.Avalonia.Demo.Pages;

public partial class PipboyControlsPage : UserControl
{
    public PipboyControlsPage()
    {
        InitializeComponent();
    }

    // ── PipboyCountdown controls ──────────────────────────────────────────────

    private void OnCountdownStart(object? sender, RoutedEventArgs e)
        => DemoCountdown.Start();

    private void OnCountdownStop(object? sender, RoutedEventArgs e)
        => DemoCountdown.Stop();

    private void OnCountdownReset(object? sender, RoutedEventArgs e)
        => DemoCountdown.Reset();

    // ── TerminalPanel typewriter replay ──────────────────────────────────────

    private void OnTerminalReplay(object? sender, RoutedEventArgs e)
    {
        // Cycle TypewriterEffect off → on to restart the reveal animation.
        TypewriterTerminal.TypewriterEffect = false;
        TypewriterTerminal.TypewriterEffect = true;
    }
}
