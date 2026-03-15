using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Pipboy.Avalonia.Demo.Pages;

public partial class PipboyControlsPage : UserControl
{
    public PipboyControlsPage()
    {
        InitializeComponent();
    }

    // ── PipboyCountdown (Seconds) ─────────────────────────────────────────────

    private void OnCountdownStart(object? sender, RoutedEventArgs e)
        => DemoCountdown.Start();

    private void OnCountdownStop(object? sender, RoutedEventArgs e)
        => DemoCountdown.Stop();

    private void OnCountdownReset(object? sender, RoutedEventArgs e)
        => DemoCountdown.Reset();

    // ── PipboyCountdown (Milliseconds) ───────────────────────────────────────

    private void OnMsCountdownStart(object? sender, RoutedEventArgs e)
        => MsCountdown.Start();

    private void OnMsCountdownStop(object? sender, RoutedEventArgs e)
        => MsCountdown.Stop();

    private void OnMsCountdownReset(object? sender, RoutedEventArgs e)
        => MsCountdown.Reset();

    // ── TerminalPanel typewriter replay ──────────────────────────────────────

    private void OnTerminalReplay(object? sender, RoutedEventArgs e)
    {
        // Cycle TypewriterEffect off → on to restart the reveal animation.
        TypewriterTerminal.TypewriterEffect = false;
        TypewriterTerminal.TypewriterEffect = true;
    }
}
