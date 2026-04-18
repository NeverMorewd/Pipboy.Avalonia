using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace Pipboy.Avalonia.Demo.Android;

[Activity(Label = "Pipboy.Avalonia.Demo", Theme = "@style/MyTheme.NoActionBar", Icon = "@drawable/icon", MainLauncher = true, Exported = true, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
}