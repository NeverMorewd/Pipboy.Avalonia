using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Pipboy.Avalonia.Demo.Pages;

public partial class ThemePage : UserControl
{
    private static readonly (PipboyPaletteStrategy Strategy, string Description)[] StrategyDescriptions =
    [
        (PipboyPaletteStrategy.Classic, "The original derivation. Border in particular can fall below WCAG's 3:1 non-text contrast minimum for some primary colors."),
        (PipboyPaletteStrategy.AccessibleContrast, "Text/TextDim/status colors held to 4.5:1 and Border/BorderFocus/Focus held to 3:1 against Surface (WCAG 2.1 AA, SC 1.4.3 / 1.4.11)."),
        (PipboyPaletteStrategy.HighContrast, "The same roles held to a stricter floor - 7:1 for text (WCAG AAA, SC 1.4.6), 4.5:1 for non-text - approximating an OS-level high-contrast mode."),
        (PipboyPaletteStrategy.PerceptuallyUniform, "Background/Surface/SurfaceHigh/Text/TextDim/Hover/Pressed/Disabled recomputed by CIE L* instead of raw HSL lightness, so they look equally bright/dark regardless of hue (the same technique behind Material Design 3's tonal palettes)."),
        (PipboyPaletteStrategy.ColorblindSafe, "Widens the CIE L* gap between Success/Warning/Error to at least 15 points if the Classic derivation left them closer, so the three stay distinguishable by lightness alone."),
    ];

    public ThemePage()
    {
        InitializeComponent();

        // Reflect whatever strategy is already active (e.g. set from another page, or by the
        // host application at startup) rather than assuming Classic.
        if (PaletteStrategyComboBox is not null)
        {
            var current = PipboyThemeManager.Instance.PaletteStrategy;
            PaletteStrategyComboBox.SelectedIndex = Array.FindIndex(StrategyDescriptions, s => s.Strategy == current);
        }
    }

    private void OnPaletteStrategyChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (PaletteStrategyComboBox?.SelectedItem is not ComboBoxItem { Tag: string tag }) return;
        if (!Enum.TryParse<PipboyPaletteStrategy>(tag, out var strategy)) return;

        PipboyThemeManager.Instance.SetPaletteStrategy(strategy);

        if (PaletteStrategyDescription is not null)
            PaletteStrategyDescription.Text = StrategyDescriptions.FirstOrDefault(s => s.Strategy == strategy).Description;
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
