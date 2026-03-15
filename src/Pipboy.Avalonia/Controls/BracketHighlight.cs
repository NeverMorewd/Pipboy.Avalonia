using Avalonia;
using Avalonia.Controls;

namespace Pipboy.Avalonia;

/// <summary>
/// A ContentControl that shows animated Pip-Boy bracket indicators ("> ... <")
/// when selected or hovered, like Fallout's menu selection style.
/// </summary>
public class BracketHighlight : ContentControl
{
    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<BracketHighlight, bool>(nameof(IsSelected), defaultValue: false);

    static BracketHighlight()
    {
        IsSelectedProperty.Changed.AddClassHandler<BracketHighlight>(
            (x, e) => x.PseudoClasses.Set(":selected", e.NewValue is true));
    }

    /// <summary>Gets or sets whether the bracket selection indicators are shown.</summary>
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }
}
