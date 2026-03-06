using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Pipboy.Avalonia.Demo;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void OnColorButtonClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string hex)
        {
            PipboyThemeManager.Instance.TrySetPrimaryColor(hex);
            if (StatusText != null)
                StatusText.Text = $"Current theme: {btn.Content} ({hex})";
        }
    }

    private void OnApplyHexColor(object? sender, RoutedEventArgs e)
    {
        var hex = HexColorBox?.Text?.Trim();
        if (string.IsNullOrEmpty(hex)) return;

        if (PipboyThemeManager.Instance.TrySetPrimaryColor(hex))
        {
            if (StatusText != null)
                StatusText.Text = $"Current theme: Custom ({hex})";
        }
        else
        {
            if (StatusText != null)
                StatusText.Text = $"Invalid color: {hex}";
        }
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
