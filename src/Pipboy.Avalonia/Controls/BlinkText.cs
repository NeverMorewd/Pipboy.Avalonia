using Avalonia;
using Avalonia.Controls;

namespace Pipboy.Avalonia;

/// <summary>
/// A ContentControl that makes its content blink at a configurable interval.
/// Uses a pure XAML animation — safe on all platforms including WASM.
/// </summary>
public class BlinkText : ContentControl
{
    public static readonly StyledProperty<bool> IsBlinkingProperty =
        AvaloniaProperty.Register<BlinkText, bool>(nameof(IsBlinking), defaultValue: false);

    public static readonly StyledProperty<double> BlinkIntervalMsProperty =
        AvaloniaProperty.Register<BlinkText, double>(nameof(BlinkIntervalMs), defaultValue: 530.0);

    static BlinkText()
    {
        IsBlinkingProperty.Changed.AddClassHandler<BlinkText>(
            (x, e) => x.PseudoClasses.Set(":blinking", e.NewValue is true));
    }

    /// <summary>Gets or sets whether the content is currently blinking.</summary>
    public bool IsBlinking
    {
        get => GetValue(IsBlinkingProperty);
        set => SetValue(IsBlinkingProperty, value);
    }

    /// <summary>
    /// Gets or sets the blink interval in milliseconds.
    /// Note: the default XAML animation uses ~530 ms (Fallout 4 cursor blink rate).
    /// </summary>
    public double BlinkIntervalMs
    {
        get => GetValue(BlinkIntervalMsProperty);
        set => SetValue(BlinkIntervalMsProperty, value);
    }
}
