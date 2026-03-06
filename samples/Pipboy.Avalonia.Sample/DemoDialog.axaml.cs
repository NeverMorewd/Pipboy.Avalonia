using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Pipboy.Avalonia.Sample;

public partial class DemoDialog : Window
{
    public DemoDialog()
    {
        InitializeComponent();
    }

    private void OnRunDiagnostic(object? sender, RoutedEventArgs e)
    {
        // In a real app this would trigger some action
        Close();
    }

    private void OnClose(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
