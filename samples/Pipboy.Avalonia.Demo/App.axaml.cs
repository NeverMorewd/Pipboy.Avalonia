using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;

namespace Pipboy.Avalonia.Demo;

public partial class App : Application
{
    // Shared notification manager — valid for both Desktop and WASM after initialization.
    internal static WindowNotificationManager? NotificationManager { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = new MainWindow();
            desktop.MainWindow = window;

            // Must be created before TemplateApplied fires so WNM can hook into
            // the AdornerLayer. Creating it lazily after Show() is too late.
            NotificationManager = new WindowNotificationManager(window)
            {
                Position = NotificationPosition.BottomRight,
                MaxItems = 4,
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            var mainView = new MainView();

            // Register BEFORE setting MainView so the event is never missed.
            // In WASM, EmbeddableControlRoot.Prepare() already ran (via AfterApplicationSetup),
            // so the content presenter may attach mainView synchronously or on the very next
            // layout pass — either way this handler fires before any user interaction.
            //
            // We add WNM to OverlayLayer (the same layer popups use) and anchor it via
            // Canvas.Right/Bottom, avoiding the AdornerLayer's BoundsProperty subscription
            // that clips WNM to 0×0 before the first render pass completes.
            mainView.AttachedToVisualTree += (_, _) =>
            {
                if (NotificationManager is not null) return;
                if (TopLevel.GetTopLevel(mainView) is not { } tl) return;

                var vlm = tl.FindDescendantOfType<VisualLayerManager>();
                var overlay = vlm?.OverlayLayer;

                if (overlay is not null)
                {
                    var wNM = new WindowNotificationManager
                    {
                        Position = NotificationPosition.BottomRight,
                        MaxItems = 4,
                    };
                    Canvas.SetRight(wNM, 0);
                    Canvas.SetBottom(wNM, 0);
                    overlay.Children.Add(wNM);
                    NotificationManager = wNM;
                }
                else
                {
                    // Fallback: let WNM install itself via AdornerLayer
                    NotificationManager = new WindowNotificationManager(tl)
                    {
                        Position = NotificationPosition.BottomRight,
                        MaxItems = 4,
                    };
                }
            };

            singleView.MainView = mainView;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
