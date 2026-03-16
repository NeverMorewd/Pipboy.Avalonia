using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Threading;

namespace Pipboy.Avalonia;

/// <summary>
/// A ContentControl styled as a Pip-Boy/Fallout terminal screen.
/// When <see cref="TypewriterEffect"/> is enabled and <see cref="ContentControl.Content"/>
/// is a string, the text is revealed character-by-character using a
/// <see cref="DispatcherTimer"/> (WASM-safe).
/// </summary>
[PseudoClasses(":typewriter")]
public class TerminalPanel : ContentControl
{
    public static readonly StyledProperty<bool> TypewriterEffectProperty =
        AvaloniaProperty.Register<TerminalPanel, bool>(nameof(TypewriterEffect), defaultValue: false);

    public static readonly StyledProperty<double> TypewriterDelayMsProperty =
        AvaloniaProperty.Register<TerminalPanel, double>(nameof(TypewriterDelayMs), defaultValue: 30.0);

    public static readonly DirectProperty<TerminalPanel, string> DisplayedTextProperty =
        AvaloniaProperty.RegisterDirect<TerminalPanel, string>(
            nameof(DisplayedText), o => o.DisplayedText);

    private string _displayedText = string.Empty;
    private string _fullText = string.Empty;
    private int _charIndex;
    private DispatcherTimer? _timer;

    static TerminalPanel()
    {
        ContentProperty.Changed.AddClassHandler<TerminalPanel>((x, _) => x.OnContentChanged());
        TypewriterEffectProperty.Changed.AddClassHandler<TerminalPanel>((x, _) => x.OnContentChanged());
        TypewriterDelayMsProperty.Changed.AddClassHandler<TerminalPanel>(
            (x, _) => x.UpdateTimerInterval());
        TypewriterEffectProperty.Changed.AddClassHandler<TerminalPanel>(
            (x, e) => x.PseudoClasses.Set(":typewriter", e.NewValue is true));
    }

    /// <summary>Gets or sets whether the typewriter reveal effect is active.</summary>
    public bool TypewriterEffect
    {
        get => GetValue(TypewriterEffectProperty);
        set => SetValue(TypewriterEffectProperty, value);
    }

    /// <summary>Gets or sets the delay in milliseconds between each revealed character.</summary>
    public double TypewriterDelayMs
    {
        get => GetValue(TypewriterDelayMsProperty);
        set => SetValue(TypewriterDelayMsProperty, value);
    }

    /// <summary>
    /// Gets the currently displayed text. When <see cref="TypewriterEffect"/> is true
    /// and content is a string, this grows character by character. Otherwise it equals
    /// the full content string immediately.
    /// </summary>
    public string DisplayedText
    {
        get => _displayedText;
        private set => SetAndRaise(DisplayedTextProperty, ref _displayedText, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (TypewriterEffect && _charIndex < _fullText.Length)
            StartTypewriter();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        StopTypewriter();
    }

    private void OnContentChanged()
    {
        StopTypewriter();
        _fullText = Content is string s ? s : string.Empty;
        _charIndex = 0;

        if (!TypewriterEffect || string.IsNullOrEmpty(_fullText))
        {
            DisplayedText = _fullText;
            return;
        }

        DisplayedText = string.Empty;
        StartTypewriter();
    }

    private void StartTypewriter()
    {
        if (_timer is not null || string.IsNullOrEmpty(_fullText)) return;
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(Math.Max(1.0, TypewriterDelayMs))
        };
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    private void StopTypewriter()
    {
        if (_timer is null) return;
        _timer.Stop();
        _timer.Tick -= OnTimerTick;
        _timer = null;
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (_charIndex < _fullText.Length)
        {
            _charIndex++;
            DisplayedText = _fullText[.._charIndex];
        }
        else
        {
            StopTypewriter();
        }
    }

    private void UpdateTimerInterval()
    {
        if (_timer is not null)
            _timer.Interval = TimeSpan.FromMilliseconds(Math.Max(1.0, TypewriterDelayMs));
    }
}
