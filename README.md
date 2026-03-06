# Pipboy.Avalonia

A Fallout 4 Pip-Boy inspired theme library for [Avalonia UI](https://avaloniaui.net/).

Sharp corners, monochromatic palette, retro-green phosphor glow — drop it in as your sole application theme and every standard control gets the Vault-Tec treatment.

---

## Features

- **Full control coverage** — Button, TextBox, CheckBox, RadioButton, ToggleButton, ToggleSwitch, Slider, ProgressBar, ListBox, ComboBox, TreeView, TabControl, Menu, ContextMenu, Expander, ScrollViewer, NumericUpDown, DatePicker, TimePicker, CalendarDatePicker, AutoCompleteBox, SplitButton, DropDownButton, HyperlinkButton, RepeatButton, ToolTip, SplitView, GridSplitter, DataValidationErrors, and more.
- **Runtime color switching** — change the primary color at any time; all 18 brush resources update instantly via `SolidColorBrush.Color` mutation — no layout passes triggered.
- **Monochromatic palette** — the entire color system is derived from a single HSL primary color. Hover, pressed, selection, border, and background variants are computed automatically.
- **No rounded corners** — all controls use `CornerRadius="0"` by design.
- **Zero third-party dependencies** — only `Avalonia` is referenced.
- **AOT / trimming compatible** — compiled XAML bindings, `IsTrimmable`, and `IsAotCompatible` all enabled.
- **`netstandard2.0` + `net8.0`** dual targets.
- **Typography utility classes** — `h1`, `h2`, `dim`, `accent`, `error`, `warning`, `success` for `TextBlock`; `pipboy-panel`, `pipboy-surface` for `Border`.

---

## Installation

```
dotnet add package Pipboy.Avalonia
```

---

## Quick Start

### 1. Apply the theme in `App.axaml`

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:pipboy="clr-namespace:Pipboy.Avalonia;assembly=Pipboy.Avalonia"
             x:Class="MyApp.App">
  <Application.Styles>
    <pipboy:PipboyTheme />
  </Application.Styles>
</Application>
```

That's it. `PipboyTheme` is a self-contained `Styles` collection — no base Fluent/Simple theme required.

### 2. (Optional) Change the primary color at startup

```csharp
// In App.axaml.cs or Program.cs
PipboyThemeManager.Instance.SetColor(Color.Parse("#FFA500")); // Amber
```

### 3. (Optional) Change color at runtime

```csharp
PipboyThemeManager.Instance.SetColor(Color.Parse("#00BFFF")); // Blue
```

Subscribe to `ThemeColorChanged` if you need to react to changes:

```csharp
PipboyThemeManager.Instance.ThemeColorChanged += (_, e) =>
{
    // e.Palette exposes all computed colors
};
```

---

## Typography Classes

Apply directly on `TextBlock`:

| Class | Effect |
|-------|--------|
| `h1` | 20 px, bold, primary color |
| `h2` | 16 px (PipboyFontSizeLarge), bold, primary color |
| `dim` | Dimmed foreground (`PipboyTextDimBrush`) |
| `accent` | Primary color foreground |
| `error` | Error red foreground |
| `warning` | Warning amber foreground |
| `success` | Success green foreground |

```xml
<TextBlock Classes="h2" Text="INVENTORY"/>
<TextBlock Classes="dim" Text="Weight: 12.4 lbs"/>
<TextBlock Classes="error" Text="ENCUMBERED"/>
```

## Layout Classes

Apply on `Border`:

| Class | Effect |
|-------|--------|
| `pipboy-panel` | Elevated surface (`PipboySurfaceHighBrush`) with border + 8 px padding |
| `pipboy-surface` | Flat surface (`PipboySurfaceBrush`) with border |

```xml
<Border Classes="pipboy-panel">
  <TextBlock Text="SECTION CONTENT"/>
</Border>
```

---

## Design Tokens

All tokens are available as `{DynamicResource}` in XAML:

### Brushes

| Resource | Description |
|----------|-------------|
| `PipboyPrimaryBrush` | Primary brand color |
| `PipboyPrimaryLightBrush` | Lighter variant (highlights) |
| `PipboyPrimaryDarkBrush` | Darker variant (selected background) |
| `PipboyBackgroundBrush` | Window / deepest background |
| `PipboySurfaceBrush` | Card / panel background |
| `PipboySurfaceHighBrush` | Elevated panel background |
| `PipboyTextBrush` | Primary text |
| `PipboyTextDimBrush` | Secondary / label text |
| `PipboyBorderBrush` | Default control border |
| `PipboyBorderFocusBrush` | Focused control border |
| `PipboyHoverBrush` | Hover state background |
| `PipboyPressedBrush` | Pressed state background |
| `PipboySelectionBrush` | Selected item background |
| `PipboyFocusBrush` | Focus ring color |
| `PipboyDisabledBrush` | Disabled foreground |
| `PipboyErrorBrush` | Validation error (fixed red) |
| `PipboyWarningBrush` | Warning (fixed amber) |
| `PipboySuccessBrush` | Success (fixed green) |

### Font Sizes

| Resource | Value |
|----------|-------|
| `PipboyFontSizeSmall` | 11 |
| `PipboyFontSize` | 13 |
| `PipboyFontSizeLarge` | 16 |

### Other

| Resource | Description |
|----------|-------------|
| `PipboyFontFamily` | `Consolas, Courier New, monospace` |
| `PipboyPrimaryColor` | Raw `Color` value of the primary |
| `PipboyBackgroundColor` | Raw `Color` of the background |
| `PipboyTextColor` | Raw `Color` of the text |

---

## Runtime Color Switching — How It Works

`PipboyTheme` registers 18 `SolidColorBrush` instances as `DynamicResource` entries. When `PipboyThemeManager.SetColor()` is called, `PipboyColorPalette` computes the full HSL-derived palette and `OnThemeColorChanged` mutates each brush's `.Color` in place. Avalonia's reactive binding system propagates the change to every bound control automatically — no re-layout, no template re-application.

```
SetColor(hex)
  └─ PipboyColorPalette.FromHex()   — HSL-derive all 15 palette colors
       └─ ThemeColorChanged event
            └─ PipboyTheme.OnThemeColorChanged()
                 └─ brush.Color = newColor  (× 15 brushes)
                      └─ DynamicResource invalidation → UI updates
```

`PipboyTheme` implements `IDisposable`. If you remove the theme from `Application.Styles` at runtime, call `Dispose()` to unsubscribe from `ThemeColorChanged` and avoid a memory leak.

---

## Supported Avalonia Version

**11.2.3** (tested). Compatible with Avalonia 11.x.

---

## License

MIT
