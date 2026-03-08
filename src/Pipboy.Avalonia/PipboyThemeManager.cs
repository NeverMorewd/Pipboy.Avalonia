using System;
using Avalonia.Media;

namespace Pipboy.Avalonia;

/// <summary>
/// Event arguments for theme color change notifications.
/// </summary>
public sealed class ThemeColorChangedEventArgs : EventArgs
{
    public PipboyColorPalette Palette { get; }

    public ThemeColorChangedEventArgs(PipboyColorPalette palette)
    {
        Palette = palette;
    }
}

/// <summary>
/// Singleton manager for the Pipboy theme.
/// Provides API to change the primary color at runtime and raises events for UI updates.
/// </summary>
public sealed class PipboyThemeManager
{
    private static readonly Lazy<PipboyThemeManager> _instance = new(() => new PipboyThemeManager());

    /// <summary>Gets the singleton instance of the theme manager.</summary>
    public static PipboyThemeManager Instance => _instance.Value;

    // Default Pipboy green from Fallout 4
    private static readonly Color DefaultPrimaryColor = Color.Parse("#15FF52");

    private Color _primaryColor;
    private PipboyColorPalette _palette;

    /// <summary>Raised when the theme primary color changes.</summary>
    public event EventHandler<ThemeColorChangedEventArgs>? ThemeColorChanged;

    private PipboyThemeManager()
    {
        _primaryColor = DefaultPrimaryColor;
        _palette = new PipboyColorPalette(_primaryColor);
    }

    /// <summary>Gets the current primary color.</summary>
    public Color PrimaryColor => _primaryColor;

    /// <summary>Gets the current color palette.</summary>
    public PipboyColorPalette Palette => _palette;

    /// <summary>
    /// Sets the primary color and regenerates the palette.
    /// Raises <see cref="ThemeColorChanged"/> if the color changed.
    /// </summary>
    public void SetPrimaryColor(Color color)
    {
        if (_primaryColor == color) return;
        _primaryColor = color;
        _palette = new PipboyColorPalette(color);
        ThemeColorChanged?.Invoke(this, new ThemeColorChangedEventArgs(_palette));
    }

    /// <summary>
    /// Attempts to set the primary color from a hex string (e.g., "#15FF52").
    /// Returns false if the string is not a valid color.
    /// </summary>
    public bool TrySetPrimaryColor(string hexColor)
    {
        if (string.IsNullOrWhiteSpace(hexColor)) return false;
        try
        {
            var color = Color.Parse(hexColor);
            SetPrimaryColor(color);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Resets the theme to the default Pipboy green color.</summary>
    public void ResetToDefault() => SetPrimaryColor(DefaultPrimaryColor);
}
