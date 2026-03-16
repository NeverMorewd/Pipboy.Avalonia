using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace Pipboy.Avalonia;

/// <summary>
/// A Pip-Boy themed borderless panel / floating window control.
/// Provides a title bar with bracket decoration, an optional footer,
/// and an optional close button that raises <see cref="ClosedEvent"/>.
/// Use <c>Classes="accent"</c> for a bright primary-colour border
/// or <c>Classes="warning"</c> for an amber alert variant.
/// </summary>
[PseudoClasses(":closable")]
public class PipboyPanel : HeaderedContentControl
{
    // ── Footer ─────────────────────────────────────────────────────────────
    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<PipboyPanel, object?>(nameof(Footer));

    // ── IsClosable ──────────────────────────────────────────────────────────
    public static readonly StyledProperty<bool> IsClosableProperty =
        AvaloniaProperty.Register<PipboyPanel, bool>(nameof(IsClosable), defaultValue: false);

    // ── Closed routed event ─────────────────────────────────────────────────
    public static readonly RoutedEvent<RoutedEventArgs> ClosedEvent =
        RoutedEvent.Register<PipboyPanel, RoutedEventArgs>(nameof(Closed), RoutingStrategies.Bubble);

    static PipboyPanel()
    {
        IsClosableProperty.Changed.AddClassHandler<PipboyPanel>(
            (x, e) => x.PseudoClasses.Set(":closable", e.NewValue is true));
    }

    /// <summary>Gets or sets content placed in the footer bar (e.g. action buttons).</summary>
    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    /// <summary>When <c>true</c>, shows a <c>[X]</c> close button in the title bar.</summary>
    public bool IsClosable
    {
        get => GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    /// <summary>
    /// Raised when the user clicks the close button (<c>PART_CloseButton</c>).
    /// The host is responsible for hiding or removing the panel in response.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? Closed
    {
        add    => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (e.NameScope.Find<Button>("PART_CloseButton") is { } btn)
            btn.Click += (_, _) => RaiseEvent(new RoutedEventArgs(ClosedEvent));
    }
}
