using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using Pipboy.Avalonia.Demo;

namespace Pipboy.Avalonia.Demo.Android;

[Activity(
    Label = "Pipboy.Avalonia.Demo",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        => base.CustomizeAppBuilder(builder)
               .LogToTrace();
}
