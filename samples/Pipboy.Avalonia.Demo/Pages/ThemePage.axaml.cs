using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Pipboy.Avalonia.Demo.Pages;

public partial class ThemePage : UserControl
{
    public ThemePage()
    {
        InitializeComponent();

        // Reflect whatever strategy is already active (e.g. set from another page, or by the
        // host application at startup) rather than assuming Classic.
        if (AccessibleContrastCheckBox is not null)
            AccessibleContrastCheckBox.IsChecked = PipboyThemeManager.Instance.PaletteStrategy == PipboyPaletteStrategy.AccessibleContrast;
    }

    private void OnAccessibleContrastToggled(object? sender, RoutedEventArgs e)
    {
        var strategy = AccessibleContrastCheckBox?.IsChecked == true
            ? PipboyPaletteStrategy.AccessibleContrast
            : PipboyPaletteStrategy.Classic;
        PipboyThemeManager.Instance.SetPaletteStrategy(strategy);
    }

    private void OnColorPresetClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string hex)
        {
            PipboyThemeManager.Instance.TrySetPrimaryColor(hex);
            StatusText?.Text = $"Current theme: {btn.Content} ({hex})";
        }
    }

    private void OnApplyHexColor(object? sender, RoutedEventArgs e)
    {
        var hex = HexColorBox?.Text?.Trim();
        if (string.IsNullOrEmpty(hex)) return;

        if (PipboyThemeManager.Instance.TrySetPrimaryColor(hex))
        {
            StatusText?.Text = $"Current theme: Custom ({hex})";
        }
        else
        {
            StatusText?.Text = $"Invalid color: {hex}";
        }
    }
}
