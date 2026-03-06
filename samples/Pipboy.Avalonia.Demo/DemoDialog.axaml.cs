using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Pipboy.Avalonia.Demo;

public partial class DemoDialog : Window
{
    public DemoDialog()
    {
        InitializeComponent();
    }

    private void OnRunDiagnostic(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnClose(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
