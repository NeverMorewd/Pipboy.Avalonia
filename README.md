# Pipboy.Avalonia

[![Built with Claude Code](https://img.shields.io/badge/Built%20with-Claude%20Code-blueviolet?logo=anthropic)](https://claude.ai/claude-code)

A Fallout 4 Pip-Boy inspired theme library for [Avalonia UI](https://avaloniaui.net/).

Sharp corners, monochromatic phosphor palette, retro terminal aesthetic — drop it in as your sole application theme and every standard control gets the Vault-Tec treatment.

> A significant portion of this codebase was written with [Claude Code](https://claude.ai/claude-code).

---

## Screenshots

<!-- Desktop -->
![Desktop overview](docs/images/screenshot-overview.png)

<!-- Runtime color switching -->
![Color switching](docs/images/screenshot-colors.png)

---

## Features

- **Full control coverage** — Button, RepeatButton, HyperlinkButton, SplitButton, DropDownButton, TextBox, CheckBox, RadioButton, ToggleButton, ToggleSwitch, Slider, ProgressBar, ScrollBar, ListBox, ComboBox, TreeView, TabControl, Menu, ContextMenu, Expander, NumericUpDown, AutoCompleteBox, DatePicker, TimePicker, CalendarDatePicker, SplitView, GridSplitter, ToolTip, FlyoutPresenter, DataValidationErrors, Notification, and more.
- **Runtime color switching** — change the primary color at any time; all brush resources update instantly via `SolidColorBrush.Color` mutation — no layout passes triggered.
- **Monochromatic palette** — the entire color system is derived from a single HSL primary color. Hover, pressed, selection, border, and background variants are computed automatically.
- **No rounded corners** — all controls use `CornerRadius="0"` by design.
- **Zero third-party dependencies** — only `Avalonia` is referenced.
- **AOT / trimming compatible** — compiled XAML bindings, `IsTrimmable`, and `IsAotCompatible` all enabled.
- **`net10.0`** primary target.
- **Multi-platform** — Desktop (Windows, macOS, Linux), Browser (WASM), Android, iOS.
- **Typography utility classes** — `h1`, `h2`, `dim`, `accent`, `error`, `warning`, `success` for `TextBlock`; `pipboy-panel`, `pipboy-surface` for `Border`.

---

## Platform Support

| Platform | Project suffix | Notes |
|----------|---------------|-------|
| Windows / macOS / Linux | `.Desktop` | `IClassicDesktopStyleApplicationLifetime` |
| Browser (WASM) | `.Browser` | `net8.0-browser`, `Avalonia.Browser` |
| Android | `.Android` | `net8.0-android` |
| iOS | `.iOS` | `net8.0-ios` |

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

`PipboyTheme` is a self-contained `Styles` collection — no base Fluent/Simple theme required.

### 2. (Optional) Set the primary color at startup

```csharp
// In App.axaml.cs or Program.cs — before the window is shown
PipboyThemeManager.Instance.SetPrimaryColor(Color.Parse("#FFA500")); // Amber
```

The default color is phosphor green (`#4CAF50`-ish).

### 3. (Optional) Change color at runtime

```csharp
PipboyThemeManager.Instance.SetPrimaryColor(Color.Parse("#00BFFF")); // Blue
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
| `h2` | 16 px, bold, primary color |
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

All tokens are available as `{DynamicResource}` in XAML.

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
| `PipboyErrorBrush` | Error severity (hue-derived, near-white lightness) |
| `PipboyWarningBrush` | Warning severity (hue-derived, bright lightness) |
| `PipboySuccessBrush` | Success severity (hue-derived, mid lightness) |

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

`PipboyTheme` registers `SolidColorBrush` instances as `DynamicResource` entries. When `PipboyThemeManager.SetPrimaryColor()` is called, `PipboyColorPalette` computes the full HSL-derived palette and `OnThemeColorChanged` mutates each brush's `.Color` in place. Avalonia's reactive binding system propagates the change to every bound control automatically — no re-layout, no template re-application.

```
SetPrimaryColor(color)
  └─ PipboyColorPalette(color)      — HSL-derive all palette colors
       └─ ThemeColorChanged event
            └─ PipboyTheme.OnThemeColorChanged()
                 └─ brush.Color = newColor  (× 18 brushes)
                      └─ DynamicResource invalidation → UI updates
```

`PipboyTheme` implements `IDisposable`. If you remove the theme from `Application.Styles` at runtime, call `Dispose()` to unsubscribe from `ThemeColorChanged` and avoid a memory leak.

---

## Demo App

The `samples/` directory contains a full demo app targeting all supported platforms:

| Page | What it shows |
|------|--------------|
| Overview | Side-by-side palette & key controls |
| Buttons | Button, RepeatButton, HyperlinkButton, SplitButton, DropDownButton |
| Text Input | TextBox, AutoCompleteBox, NumericUpDown with validation |
| Toggles | CheckBox, RadioButton, ToggleButton, ToggleSwitch |
| Sliders & Progress | Slider, ProgressBar (horizontal + vertical + indeterminate) |
| Lists | ListBox, TreeView |
| ComboBox | ComboBox with various item counts |
| Tab Control | TabControl, TabItem |
| Menu | MenuBar, ContextMenu, Flyout |
| Expander | Expander, SplitView, GridSplitter |
| Date & Time | DatePicker, TimePicker, CalendarDatePicker |
| Typography | All TextBlock classes + layout Border classes |
| Cards | `pipboy-panel` / `pipboy-surface` layout |
| Notifications | WindowNotificationManager (all four types) |
| Theme | Runtime color picker |
| Window | Window chrome, dialogs |

---

## Supported Avalonia Version

**11.3.12** (tested). Compatible with Avalonia 11.x.

---

## License

MIT
