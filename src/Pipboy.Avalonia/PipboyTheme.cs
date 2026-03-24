using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;

namespace Pipboy.Avalonia;

/// <summary>
/// The main Pipboy theme class. Add to <c>Application.Styles</c> to apply the theme globally.
/// Implements <see cref="IDisposable"/> — call <see cref="Dispose"/> if you remove the theme
/// from <c>Application.Styles</c> at runtime to avoid a memory leak.
/// <example>
/// In App.axaml:
/// <code>
/// &lt;Application.Styles&gt;
///     &lt;pipboy:PipboyTheme /&gt;
/// &lt;/Application.Styles&gt;
/// </code>
/// In code:
/// <code>
/// this.Styles.Add(new PipboyTheme());
/// </code>
/// </example>
/// </summary>
public partial class PipboyTheme : Styles, IDisposable
{
    private readonly PipboyThemeManager _manager;

    // Mutable brush instances — updating .Color propagates to all bound controls
    private readonly SolidColorBrush _primaryBrush;
    private readonly SolidColorBrush _primaryLightBrush;
    private readonly SolidColorBrush _primaryDarkBrush;
    private readonly SolidColorBrush _backgroundBrush;
    private readonly SolidColorBrush _surfaceBrush;
    private readonly SolidColorBrush _surfaceHighBrush;
    private readonly SolidColorBrush _textBrush;
    private readonly SolidColorBrush _textDimBrush;
    private readonly SolidColorBrush _hoverBrush;
    private readonly SolidColorBrush _pressedBrush;
    private readonly SolidColorBrush _disabledBrush;
    private readonly SolidColorBrush _focusBrush;
    private readonly SolidColorBrush _selectionBrush;
    private readonly SolidColorBrush _borderBrush;
    private readonly SolidColorBrush _borderFocusBrush;

    // Semantic status brushes — same hue as primary, update with theme changes
    private readonly SolidColorBrush _errorBrush;
    private readonly SolidColorBrush _warningBrush;
    private readonly SolidColorBrush _successBrush;

    public PipboyTheme(IServiceProvider? serviceProvider = null)
    {
        _manager = PipboyThemeManager.Instance;
        var p = _manager.Palette;

        // Create brush instances
        _primaryBrush = new SolidColorBrush(p.Primary);
        _primaryLightBrush = new SolidColorBrush(p.PrimaryLight);
        _primaryDarkBrush = new SolidColorBrush(p.PrimaryDark);
        _backgroundBrush = new SolidColorBrush(p.Background);
        _surfaceBrush = new SolidColorBrush(p.Surface);
        _surfaceHighBrush = new SolidColorBrush(p.SurfaceHigh);
        _textBrush = new SolidColorBrush(p.Text);
        _textDimBrush = new SolidColorBrush(p.TextDim);
        _hoverBrush = new SolidColorBrush(p.Hover);
        _pressedBrush = new SolidColorBrush(p.Pressed);
        _disabledBrush = new SolidColorBrush(p.Disabled);
        _focusBrush = new SolidColorBrush(p.Focus);
        _selectionBrush = new SolidColorBrush(p.Selection);
        _borderBrush = new SolidColorBrush(p.Border);
        _borderFocusBrush = new SolidColorBrush(p.BorderFocus);

        _errorBrush   = new SolidColorBrush(p.Error);
        _warningBrush = new SolidColorBrush(p.Warning);
        _successBrush = new SolidColorBrush(p.Success);

        // Register resources so AXAML styles can reference them via {DynamicResource}
        Resources["PipboyPrimaryBrush"] = _primaryBrush;
        Resources["PipboyPrimaryLightBrush"] = _primaryLightBrush;
        Resources["PipboyPrimaryDarkBrush"] = _primaryDarkBrush;
        Resources["PipboyBackgroundBrush"] = _backgroundBrush;
        Resources["PipboySurfaceBrush"] = _surfaceBrush;
        Resources["PipboySurfaceHighBrush"] = _surfaceHighBrush;
        Resources["PipboyTextBrush"] = _textBrush;
        Resources["PipboyTextDimBrush"] = _textDimBrush;
        Resources["PipboyHoverBrush"] = _hoverBrush;
        Resources["PipboyPressedBrush"] = _pressedBrush;
        Resources["PipboyDisabledBrush"] = _disabledBrush;
        Resources["PipboyFocusBrush"] = _focusBrush;
        Resources["PipboySelectionBrush"] = _selectionBrush;
        Resources["PipboyBorderBrush"] = _borderBrush;
        Resources["PipboyBorderFocusBrush"] = _borderFocusBrush;

        // Semantic status brushes
        Resources["PipboyErrorBrush"]   = _errorBrush;
        Resources["PipboyWarningBrush"] = _warningBrush;
        Resources["PipboySuccessBrush"] = _successBrush;

        // Also expose raw Color values for advanced use
        Resources["PipboyPrimaryColor"]    = p.Primary;
        Resources["PipboyBackgroundColor"] = p.Background;
        Resources["PipboyTextColor"]       = p.Text;
        Resources["PipboyScanBeamColor"]   = Color.FromArgb(40, p.Primary.R, p.Primary.G, p.Primary.B);

        // Font design tokens
        Resources["PipboyFontFamily"]      = new FontFamily("Consolas,Courier New,monospace");
        Resources["PipboyFontSize"]        = 13.0;
        Resources["PipboyFontSizeXSmall"]  = 10.0;
        Resources["PipboyFontSizeSmall"]   = 11.0;
        Resources["PipboyFontSizeLarge"]   = 16.0;

        // Spacing / sizing design tokens
        Resources["PipboyControlHeight"]      = 30.0;
        Resources["PipboyTreeViewItemIndent"] = 16.0;
        Resources["PipboyPickerRowHeight"]    = new GridLength(29);
        Resources["PipboyPickerItemHeight"]   = 40.0;
        Resources["PipboyPopupMaxHeight"]     = 200.0;

        // Opacity design tokens
        Resources["PipboyDisabledOpacity"] = 0.45;
        Resources["PipboyDimOpacity"]      = 0.7;

        // Stroke design tokens
        Resources["PipboyIconStrokeThickness"] = 1.5;

        // Load compiled AXAML styles — AvaloniaXamlLoader.Load uses the compiled (NativeAOT-safe)
        // version generated from PipboyTheme.axaml; the StyleInclude chain inside that AXAML file
        // is resolved at compile time, so no runtime URI lookup is required.
        AvaloniaXamlLoader.Load(serviceProvider, this);

        // Subscribe to runtime color changes
        _manager.ThemeColorChanged += OnThemeColorChanged;
    }

    private void OnThemeColorChanged(object? sender, ThemeColorChangedEventArgs e)
    {
        var p = e.Palette;

        // Updating brush Color is reactive — all bindings update automatically
        _primaryBrush.Color = p.Primary;
        _primaryLightBrush.Color = p.PrimaryLight;
        _primaryDarkBrush.Color = p.PrimaryDark;
        _backgroundBrush.Color = p.Background;
        _surfaceBrush.Color = p.Surface;
        _surfaceHighBrush.Color = p.SurfaceHigh;
        _textBrush.Color = p.Text;
        _textDimBrush.Color = p.TextDim;
        _hoverBrush.Color = p.Hover;
        _pressedBrush.Color = p.Pressed;
        _disabledBrush.Color = p.Disabled;
        _focusBrush.Color = p.Focus;
        _selectionBrush.Color = p.Selection;
        _borderBrush.Color = p.Border;
        _borderFocusBrush.Color = p.BorderFocus;
        _errorBrush.Color   = p.Error;
        _warningBrush.Color = p.Warning;
        _successBrush.Color = p.Success;

        // Update raw Color resources
        Resources["PipboyPrimaryColor"]    = p.Primary;
        Resources["PipboyBackgroundColor"] = p.Background;
        Resources["PipboyTextColor"]       = p.Text;
        Resources["PipboyScanBeamColor"]   = Color.FromArgb(40, p.Primary.R, p.Primary.G, p.Primary.B);
    }

    public void Dispose()
    {
        _manager.ThemeColorChanged -= OnThemeColorChanged;
        GC.SuppressFinalize(this);
    }
}
