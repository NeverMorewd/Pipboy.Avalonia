using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Pipboy.Avalonia.Demo.Pages;

public partial class FeedbackPage : UserControl
{
    public FeedbackPage()
    {
        InitializeComponent();
    }

    private void OnFlyoutClose(object? sender, RoutedEventArgs e)
    {
        PopupFlyoutBtn.Flyout?.Hide();
        FlyoutStatusText.Text = "Action cancelled.";
    }

    private void OnFlyoutConfirm(object? sender, RoutedEventArgs e)
    {
        PopupFlyoutBtn.Flyout?.Hide();
        FlyoutStatusText.Text = "Action confirmed. Self-destruct initiated.";
    }
}
