using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace Pipboy.Avalonia;

/// <summary>
/// Custom Pip-Boy themed title bar for use inside a <see cref="PipboyWindow"/>.
/// Provides a draggable title area, optional extra <see cref="TitleBarContent"/>,
/// and minimize / maximize-restore / close buttons.
/// </summary>
[PseudoClasses(":maximized")]
public class PipboyTitleBar : TemplatedControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<PipboyTitleBar, string>(nameof(Title), defaultValue: string.Empty);

    public static readonly StyledProperty<object?> TitleBarContentProperty =
        AvaloniaProperty.Register<PipboyTitleBar, object?>(nameof(TitleBarContent));

    private Window? _window;

    /// <summary>Gets or sets the window title displayed in the title bar.</summary>
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets optional extra content placed in the centre of the title bar
    /// (e.g. a version badge, status indicator, or breadcrumb).
    /// </summary>
    public object? TitleBarContent
    {
        get => GetValue(TitleBarContentProperty);
        set => SetValue(TitleBarContentProperty, value);
    }

    // ── Visual-tree lifecycle ────────────────────────────────────────────────

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (e.Root is Window w)
        {
            _window = w;
            _window.PropertyChanged += OnWindowPropertyChanged;
            UpdateMaximizedState(_window.WindowState);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (_window != null)
        {
            _window.PropertyChanged -= OnWindowPropertyChanged;
            _window = null;
        }
    }

    // ── Template parts ───────────────────────────────────────────────────────

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (e.NameScope.Find<Control>("PART_DragArea") is { } drag)
            drag.PointerPressed += OnDragPointerPressed;

        if (e.NameScope.Find<Button>("PART_MinimizeButton") is { } min)
            min.Click += (_, _) =>
            {
                if (_window is not null) _window.WindowState = WindowState.Minimized;
            };

        if (e.NameScope.Find<Button>("PART_MaxRestoreButton") is { } maxRestore)
            maxRestore.Click += (_, _) => ToggleMaximize();

        if (e.NameScope.Find<Button>("PART_CloseButton") is { } close)
            close.Click += (_, _) => _window?.Close();
    }

    // ── Drag & double-click to maximize ─────────────────────────────────────

    private void OnDragPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_window is null) return;
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

        if (e.ClickCount >= 2)
        {
            ToggleMaximize();
            e.Handled = true;
        }
        else
        {
            _window.BeginMoveDrag(e);
        }
    }

    private void ToggleMaximize()
    {
        if (_window is null) return;
        _window.WindowState = _window.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    // ── Window state ─────────────────────────────────────────────────────────

    private void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Window.WindowStateProperty && e.NewValue is WindowState state)
            UpdateMaximizedState(state);
    }

    private void UpdateMaximizedState(WindowState state)
        => PseudoClasses.Set(":maximized", state == WindowState.Maximized);
}
