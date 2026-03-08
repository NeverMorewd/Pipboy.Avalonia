using Avalonia.Controls;
using Avalonia.Controls.Notifications;

namespace Pipboy.Avalonia.Demo;

public partial class MainWindow : Window
{
    // Created here — before the window template is applied — so the manager can
    // subscribe to TopLevel.TemplateApplied and wire itself into the AdornerLayer.
    // Creating it lazily (on first notification request) is too late: TemplateApplied
    // has already fired, the manager is never installed, and Show() does nothing.
    internal static WindowNotificationManager? NotificationManager { get; private set; }

    public MainWindow()
    {
        InitializeComponent();
        NotificationManager = new WindowNotificationManager(this)
        {
            Position = NotificationPosition.BottomRight,
            MaxItems  = 4,
        };
    }
}
