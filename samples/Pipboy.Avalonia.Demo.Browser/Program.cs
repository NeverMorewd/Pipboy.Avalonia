using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Pipboy.Avalonia.Demo;

[assembly: SupportedOSPlatform("browser")]

namespace Pipboy.Avalonia.Demo.Browser;

internal sealed partial class Program
{
    private static async Task Main(string[] args)
        => await BuildAvaloniaApp()
               .StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
                     .LogToTrace();
}
