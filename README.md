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

## Features

- **Full control coverage** — Button, RepeatButton, HyperlinkButton, SplitButton, DropDownButton, TextBox, CheckBox, RadioButton, ToggleButton, ToggleSwitch, Slider, ProgressBar, ScrollBar, ListBox, ComboBox, TreeView, TabControl, Menu, ContextMenu, Expander, NumericUpDown, AutoCompleteBox, DatePicker, TimePicker, CalendarDatePicker, SplitView, GridSplitter, ToolTip, FlyoutPresenter, DataValidationErrors, Notification, and more.
- **Runtime color switching** — change the primary color at any time; all brush resources and CRT effects update instantly.
- **Monochromatic palette** — the entire color system is derived from a single HSL primary color.
- **Purpose-built controls** — `CrtDisplay`, `PipboyWindow`, `PipboyTitleBar`, `PipboyCountdown`, `PipboyPanel`, `SegmentedBar`, `RatedAttribute`, `BracketHighlight`, `PipboyTabStrip`, `TerminalPanel`, `BlinkText`, `ScanlineOverlay`.
- **MVVM-ready** — all custom controls expose `ICommand` properties alongside routed events.
- **No rounded corners** — all controls use `CornerRadius="0"` by design.
- **Zero third-party dependencies** — only `Avalonia` is referenced.
- **AOT / trimming compatible** — compiled XAML bindings, `IsTrimmable`, and `IsAotCompatible` all enabled.
- **Multi-platform** — Desktop (Windows, macOS, Linux), Browser (WASM), Android, iOS.

---

## Platform Support

| Platform | Notes |
|----------|-------|
| Windows / macOS / Linux | `IClassicDesktopStyleApplicationLifetime` |
| Browser (WASM) | `Avalonia.Browser` |
| Android | `net8.0-android` |
| iOS | `net8.0-ios` |

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

## Supported Avalonia Version

**11.3.12** (tested). Compatible with Avalonia 11.x.

---

## License

MIT
