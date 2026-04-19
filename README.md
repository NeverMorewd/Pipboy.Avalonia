# Pipboy.Avalonia

[![Built with Claude Code](https://img.shields.io/badge/Built%20with-Claude%20Code-blueviolet?logo=anthropic)](https://claude.ai/claude-code)
[![NuGet](https://img.shields.io/nuget/v/Pipboy.Avalonia?logo=nuget)](https://www.nuget.org/packages/Pipboy.Avalonia)
[![Live Demo](https://img.shields.io/badge/Live%20Demo-GitHub%20Pages-brightgreen?logo=github)](https://nevermorewd.github.io/Pipboy.Avalonia/)

A Fallout 4 Pip-Boy inspired theme library for [Avalonia UI](https://avaloniaui.net/).

Sharp corners, monochromatic phosphor palette, retro terminal aesthetic — drop it in as your sole application theme and every standard control gets the Vault-Tec treatment.

**[▶ Try the live WASM demo](https://nevermorewd.github.io/Pipboy.Avalonia/)** — runs entirely in the browser, no install needed.

> A significant portion of this codebase was written with [Claude Code](https://claude.ai/claude-code).

---

## Screenshots

<!-- Desktop -->
![Desktop overview](docs/images/screenshot-overview.png)

<!-- Runtime color switching -->
![Color switching](docs/images/screenshot-colors.png)

![Color switching](docs/images/screenshot-map.png)

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

### 2. (Optional) Set the primary color

```csharp
// Before the window is shown, or at any time at runtime
PipboyThemeManager.Instance.SetPrimaryColor(Color.Parse("#FFA500")); // Amber
```

The default color is phosphor green. Subscribe to `ThemeColorChanged` to react to color updates.

---

## Custom Controls

Explore all controls and their properties interactively in the **[live demo](https://nevermorewd.github.io/Pipboy.Avalonia/)** or browse the [`samples/`](samples/) directory.

| Control | Description |
|---------|-------------|
| `CrtDisplay` | Layered CRT effects (scanlines, scan beam, noise, vignette, flicker) over any content |
| `PipboyWindow` | Custom-chrome window with themed title bar, icon, and system buttons |
| `PipboyTitleBar` | Standalone version of the title bar chrome for embedding in layouts |
| `PipboyPanel` | Titled, closable panel with `Closed` event and `ClosedCommand` |
| `PipboyTabStrip` | Tab-strip navigation with bracket indicators and gamepad D-Pad support |
| `PipboyCountdown` | Countdown timer with configurable precision and `CompletedCommand` |
| `SegmentedBar` | Discrete rectangular segment bar — HP/AP/RAD style |
| `RatedAttribute` | Named attribute with filled/empty dot indicators — S.P.E.C.I.A.L. style |
| `TerminalPanel` | Terminal-screen container with optional typewriter reveal effect |
| `BlinkText` | Configurable blink animation wrapper (pure XAML, WASM safe) |
| `ScanlineOverlay` | Decorator that draws CRT scanlines over its child |
| `BracketHighlight` | Animated `> ... <` bracket indicators on hover or selection |

---

## Design Tokens

All tokens are available as `{DynamicResource}` in XAML and update automatically when the primary color changes.

### Brushes

| Resource Key | Description |
|---|---|
| `PipboyPrimaryBrush` | Primary brand color |
| `PipboyPrimaryLightBrush` | Lighter variant (+0.25 lightness) |
| `PipboyPrimaryDarkBrush` | Darker variant (−0.25 lightness) |
| `PipboyBackgroundBrush` | Window / deepest background |
| `PipboySurfaceBrush` | Default control surface |
| `PipboySurfaceHighBrush` | Elevated / prominent surface |
| `PipboyTextBrush` | Primary text |
| `PipboyTextDimBrush` | Secondary / label text |
| `PipboyBorderBrush` | Default control border |
| `PipboyBorderFocusBrush` | Focused control border |
| `PipboyHoverBrush` | Hover state background |
| `PipboyPressedBrush` | Pressed state background |
| `PipboySelectionBrush` | Selected item background |
| `PipboyFocusBrush` | Focus ring color |
| `PipboyDisabledBrush` | Disabled foreground |
| `PipboyErrorBrush` | Error severity |
| `PipboyWarningBrush` | Warning severity |
| `PipboySuccessBrush` | Success severity |

### Colors (raw `Color` values)

| Resource Key | Description |
|---|---|
| `PipboyPrimaryColor` | Raw `Color` of the primary |
| `PipboyBackgroundColor` | Raw `Color` of the background |
| `PipboyTextColor` | Raw `Color` of the text |

### Typography

| Resource Key | Value |
|---|---|
| `PipboyFontFamily` | `Consolas, Courier New, monospace` |
| `PipboyFontSizeXSmall` | `10` |
| `PipboyFontSizeSmall` | `11` |
| `PipboyFontSize` | `13` |
| `PipboyFontSizeLarge` | `16` |

---

## License

MIT
