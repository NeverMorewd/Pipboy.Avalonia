using Avalonia.Controls;

namespace Pipboy.Avalonia.Demo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // WindowNotificationManager is now created in App.OnFrameworkInitializationCompleted
        // so it can be set up for both Desktop (Window) and WASM (TopLevel) in one place.
    }
}
