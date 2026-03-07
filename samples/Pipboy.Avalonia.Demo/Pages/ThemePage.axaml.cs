using Avalonia.Controls;
using Avalonia.Interactivity;
using Pipboy.Avalonia;

namespace Pipboy.Avalonia.Demo.Pages;

public partial class ThemePage : UserControl
{
    public ThemePage()
    {
        InitializeComponent();
    }

    private void OnColorPresetClick(object? sender, RoutedEventArgs e)
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
}
