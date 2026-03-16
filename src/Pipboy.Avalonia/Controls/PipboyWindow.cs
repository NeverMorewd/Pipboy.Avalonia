using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;

namespace Pipboy.Avalonia;

/// <summary>
/// A <see cref="Window"/> subclass pre-configured for Pip-Boy borderless chrome.
/// Sets <c>ExtendClientAreaToDecorationsHint</c>, <c>NoChrome</c>, and keeps
/// <c>SystemDecorations.Full</c> so OS resize handles and snap zones remain active.
/// Pair with <see cref="PipboyTitleBar"/> (included automatically via the
/// ControlTheme in PipboyTheme) for a complete custom title bar.
/// </summary>
public class PipboyWindow : Window
{
    /// <summary>
    /// Optional extra content placed in the centre of the title bar.
    /// Bind or set from AXAML; the ControlTheme forwards this to <see cref="PipboyTitleBar"/>.
    /// </summary>
    public static readonly StyledProperty<object?> TitleBarContentProperty =
        AvaloniaProperty.Register<PipboyWindow, object?>(nameof(TitleBarContent));

    public object? TitleBarContent
    {
        get => GetValue(TitleBarContentProperty);
        set => SetValue(TitleBarContentProperty, value);
    }

    public PipboyWindow()
    {
        // Extend client area to full window bounds, suppress OS chrome,
        // but keep SystemDecorations.Full for resize handles and Win11 snap.
        ExtendClientAreaToDecorationsHint = true;
        ExtendClientAreaChromeHints       = ExtendClientAreaChromeHints.NoChrome;
        ExtendClientAreaTitleBarHeightHint = -1;
        SystemDecorations                 = SystemDecorations.Full;
    }
}
