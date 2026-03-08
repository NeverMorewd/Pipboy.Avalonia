using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;

namespace Pipboy.Avalonia.Demo.Pages;

public partial class NotificationPage : UserControl
{
    public NotificationPage()
    {
        InitializeComponent();
    }

    private void Show(string title, string message, NotificationType type)
    {
        var mgr = MainWindow.NotificationManager;
        if (mgr is null)
        {
            ToastStatus.Text = "⚠ NotificationManager unavailable.";
            return;
        }
        mgr.Show(new Notification(title, message, type));
        ToastStatus.Text = $"Sent: {type} — appears bottom-right.";
    }

    private void OnShowInfo(object? sender, RoutedEventArgs e) =>
        Show("SYSTEM INFO", "Pip-Boy OS v4.2.1 loaded. All subsystems nominal.", NotificationType.Information);

    private void OnShowSuccess(object? sender, RoutedEventArgs e) =>
        Show("QUEST COMPLETE", "\"The Nuclear Option\" finished. 500 XP earned.", NotificationType.Success);

    private void OnShowWarning(object? sender, RoutedEventArgs e) =>
        Show("LOW AMMO", "10mm rounds: 8 remaining. Switch weapon or find ammo.", NotificationType.Warning);

    private void OnShowError(object? sender, RoutedEventArgs e) =>
        Show("RADIATION ALERT", "RAD level critical: 950 rads. Take RadAway immediately.", NotificationType.Error);
}
