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
    // Deliberately empty: PipboyTheme subscribes to the process-wide
    // PipboyThemeManager.Instance.ThemeColorChanged singleton for as long as it's alive, and
    // Avalonia.Headless.XUnit does not tear down/dispose this Application between [AvaloniaFact]
    // tests. Adding it here once would leak a live subscription (holding UI-thread-affine
    // SolidColorBrush fields) across every other test in this assembly, including the plain
    // [Fact] tests in PipboyThemeManagerTests that call SetPrimaryColor from a non-Avalonia
    // thread - which then fails with "The calling thread cannot access this object because a
    // different thread owns it." Tests that actually need the Pipboy theme applied add and
    // dispose their own instance (see ProDataGridReattachTests).
    public override void Initialize()
    {
    }
}
