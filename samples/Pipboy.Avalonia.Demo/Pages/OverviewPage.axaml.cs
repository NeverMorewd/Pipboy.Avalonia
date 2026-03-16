using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Pipboy.Avalonia.Demo.Pages;

public partial class OverviewPage : UserControl
{
    public OverviewPage()
    {
        InitializeComponent();
    }

    private void OnCountdownStart(object? sender, RoutedEventArgs e)  => OverviewCountdown.Start();
    private void OnCountdownStop(object? sender, RoutedEventArgs e)   => OverviewCountdown.Stop();
    private void OnCountdownReset(object? sender, RoutedEventArgs e)  => OverviewCountdown.Reset();
}
