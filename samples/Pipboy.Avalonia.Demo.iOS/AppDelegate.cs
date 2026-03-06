using Avalonia;
using Avalonia.iOS;
using Foundation;
using Pipboy.Avalonia.Demo;
using UIKit;

namespace Pipboy.Avalonia.Demo.iOS;

[Register("AppDelegate")]
public class AppDelegate : AvaloniaAppDelegate<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        => base.CustomizeAppBuilder(builder)
               .LogToTrace();
}
