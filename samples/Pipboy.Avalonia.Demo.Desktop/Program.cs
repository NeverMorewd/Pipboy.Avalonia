using Avalonia;
using Avalonia.Dialogs;
using ReactiveUI.Avalonia;
using System;
using System.Runtime.Versioning;

namespace Pipboy.Avalonia.Demo.Desktop;

internal sealed class Program
{
    [STAThread]
    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    public static void Main(string[] args)
        => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
                     .UsePlatformDetect()
                     .UseManagedSystemDialogs()
                     .WithInterFont()
                     .LogToTrace()
                     .UseReactiveUI(_ => { });
}
