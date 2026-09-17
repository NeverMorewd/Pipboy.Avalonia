using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(Pipboy.Avalonia.Tests.UnitTestAppBuilder))]

namespace Pipboy.Avalonia.Tests;

internal sealed class UnitTestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<UnitTestApp>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = true });
}

internal sealed class UnitTestApp : Application
{
    public override void Initialize()
    {
        // Mirrors a real consumer's App.axaml (Peek's, specifically): PipboyTheme first, then
        // the ProDataGrid skin, exactly the order/registration this bug was reported against.
        Styles.Add(new global::Pipboy.Avalonia.PipboyTheme());
        Styles.Add(new global::Pipboy.Avalonia.ProDataGrid.PipboyProDataGridTheme());
    }
}
