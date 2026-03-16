using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Platform;

namespace Pipboy.Avalonia;

/// <summary>
/// A <see cref="Window"/> subclass with a Pip-Boy borderless chrome.
/// Configures client-area extension and <c>NoChrome</c> so the built-in OS
/// title bar is suppressed while resize handles and Win11 snap remain active.
/// The custom title bar is defined inline in the ControlTheme (Window.axaml)
/// and wired up here via <c>PART_*</c> template parts.
/// </summary>
[PseudoClasses(":maximized")]
public class PipboyWindow : Window
{
    public static readonly StyledProperty<object?> TitleBarContentProperty =
        AvaloniaProperty.Register<PipboyWindow, object?>(nameof(TitleBarContent));

    static PipboyWindow()
    {
        WindowStateProperty.Changed.AddClassHandler<PipboyWindow>(
            (w, e) => w.PseudoClasses.Set(":maximized", e.NewValue is WindowState.Maximized));
    }

    /// <summary>
    /// Optional extra content placed in the centre of the title bar
    /// (e.g. status indicator, version badge, breadcrumb).
    /// </summary>
    public object? TitleBarContent
    {
        get => GetValue(TitleBarContentProperty);
        set => SetValue(TitleBarContentProperty, value);
    }

    public PipboyWindow()
    {
        ExtendClientAreaToDecorationsHint  = true;
        ExtendClientAreaChromeHints        = ExtendClientAreaChromeHints.NoChrome;
        ExtendClientAreaTitleBarHeightHint = -1;
        SystemDecorations                  = SystemDecorations.Full;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (e.NameScope.Find<Control>("PART_TitleDragArea") is { } drag)
            drag.PointerPressed += OnDragPointerPressed;

        if (e.NameScope.Find<Button>("PART_MinimizeButton") is { } min)
            min.Click += (_, _) => WindowState = WindowState.Minimized;

        if (e.NameScope.Find<Button>("PART_MaxRestoreButton") is { } maxRestore)
            maxRestore.Click += (_, _) =>
                WindowState = WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;

        if (e.NameScope.Find<Button>("PART_CloseButton") is { } close)
            close.Click += (_, _) => Close();
    }

    private void OnDragPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

        if (e.ClickCount >= 2)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
            e.Handled = true;
        }
        else
        {
            BeginMoveDrag(e);
        }
    }
}
