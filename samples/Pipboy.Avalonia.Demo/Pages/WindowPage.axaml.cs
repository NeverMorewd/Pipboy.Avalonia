using Avalonia.Controls;
using Avalonia.Interactivity;
using Pipboy.Avalonia.Demo;

namespace Pipboy.Avalonia.Demo.Pages;

public partial class WindowPage : UserControl
{
    public WindowPage()
    {
        InitializeComponent();
    }

    private async void OnOpenDialog(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is Window window)
        {
            var dialog = new DemoDialog();
            await dialog.ShowDialog(window);
        }
    }
}
