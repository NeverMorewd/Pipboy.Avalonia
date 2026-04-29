using Avalonia;
using Avalonia.Browser;
using ReactiveUI.Avalonia;
using System.Runtime.Versioning;
using System.Threading.Tasks;

[assembly: SupportedOSPlatform("browser")]

namespace Pipboy.Avalonia.Demo.Browser;

internal sealed partial class Program
{
    private static async Task Main(string[] args)
        => await BuildAvaloniaApp()
               .StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
                     .LogToTrace()
                     .UseReactiveUI(_ => { });
}
