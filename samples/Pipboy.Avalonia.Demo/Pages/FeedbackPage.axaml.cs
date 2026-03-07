using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Pipboy.Avalonia.Demo.Pages;

public partial class FeedbackPage : UserControl
{
    public FeedbackPage()
    {
        InitializeComponent();
    }

    // ── Menu Flyout ──────────────────────────────────────────────────────────

    // Toggle the menu flyout programmatically (button acts as toggle).
    private void OnMenuFlyoutBtnClick(object? sender, RoutedEventArgs e)
    {
        // Button.Flyout opens automatically on click — this handler exists so we
        // can reset the status text when the menu opens.
        MenuStatusText.Text = "Menu opened — select an item or click outside to close.";
    }

    // Each MenuItem routes here; update status then close the flyout.
    private void OnMenuItemClick(object? sender, RoutedEventArgs e)
    {
        if (sender is MenuItem item)
            MenuStatusText.Text = $"Selected: {item.Header}";

        // MenuFlyout closes automatically on item click via DefaultMenuInteractionHandler.
        // Explicitly hiding here is a safeguard.
        MenuFlyoutBtn.Flyout?.Hide();
    }

    // ── Popup Flyout ─────────────────────────────────────────────────────────

    // Close button (✕) and Cancel button both dismiss without action.
    private void OnFlyoutClose(object? sender, RoutedEventArgs e)
    {
        PopupFlyoutBtn.Flyout?.Hide();
        FlyoutStatusText.Text = "Action cancelled.";
    }

    // Confirm button: perform action and dismiss.
    private void OnFlyoutConfirm(object? sender, RoutedEventArgs e)
    {
        PopupFlyoutBtn.Flyout?.Hide();
        FlyoutStatusText.Text = "Action confirmed. Self-destruct initiated.";
    }
}
