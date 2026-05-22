using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using System.Diagnostics;

namespace Pipboy.Avalonia.Demo.Pages;

public partial class WindowPage : UserControl
{
    public WindowPage()
    {
        InitializeComponent();
    }

    // ── Closable panels ─────────────────────────────────────────────────────

    private void OnPanelClosed(object? sender, RoutedEventArgs e)
    {
        if (sender is Control c) c.IsVisible = false;
    }

    private void OnRestorePanels(object? sender, RoutedEventArgs e)
    {
        InfoPanel.IsVisible = true;
        WarnPanel.IsVisible = true;
    }

    // ── Modal dialogs ────────────────────────────────────────────────────────

    private void OnShowInfoModal(object? sender, RoutedEventArgs e)
    {
        ResetModal("accent");
        ModalPanel.Header  = "SYSTEM NOTIFICATION";
        ModalPanel.Content = BuildInfoContent();
        ModalPanel.Footer  = BuildAcknowledgeFooter();
        ModalBackdrop.IsVisible = true;
    }

    private void OnShowConfirmModal(object? sender, RoutedEventArgs e)
    {
        ResetModal("accent");
        ModalPanel.Header    = "CONFIRM ACTION";
        ModalPanel.IsClosable = false;
        ModalPanel.Content   = BuildConfirmContent();
        ModalPanel.Footer    = BuildConfirmFooter();
        ModalBackdrop.IsVisible = true;
    }

    private void OnShowWarningModal(object? sender, RoutedEventArgs e)
    {
        ResetModal("warning");
        ModalPanel.Header  = "CRITICAL ALERT";
        ModalPanel.Content = BuildWarningContent();
        ModalPanel.Footer  = BuildAcknowledgeFooter();
        ModalBackdrop.IsVisible = true;
    }
    private void OnShowTransparent(object? sender, RoutedEventArgs e)
    {
        Window window = new()
        {
            WindowDecorations = WindowDecorations.None,
            ExtendClientAreaTitleBarHeightHint = -1,
            Topmost = true,
            ShowActivated = false,
            ShowInTaskbar = false,
            IsHitTestVisible = true,
            IsEnabled = false,
            Background = Brushes.Transparent,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Height = (double) width.Value.GetValueOrDefault(),
            Width = (double) height.Value.GetValueOrDefault(),
            MinHeight = 0,
            MinWidth = 0,
        };
        var innerPanel = new DockPanel { Background = Brushes.Transparent };

        var innerBorder = new Border
        {
            BorderThickness = new Thickness(1.0),
            BorderBrush = new SolidColorBrush(PipboyThemeManager.Instance.PrimaryColor),
            CornerRadius = new CornerRadius(0.0),
            Child = innerPanel,
            Background = Brushes.Transparent,
        };

        window.Content = innerBorder;
        window.Show();
        

        Application.Current!.Dispatcher.Invoke(() => 
        {
            Debug.WriteLine($"{window.ClientSize}");
            Debug.WriteLine($"{window.Bounds}");
            innerPanel.Children.Add(new  TextBlock 
            {   Text = $"{window.ClientSize}",
                TextWrapping = TextWrapping.Wrap ,
                FontSize = 9 
            });
        },DispatcherPriority.Loaded);

    }
    private void ResetModal(string cssClass)
    {
        ModalPanel.Classes.Clear();
        ModalPanel.Classes.Add(cssClass);
        ModalPanel.IsClosable = true;
        ModalPanel.Closed -= HideModal;
        ModalPanel.Closed += HideModal;
    }

    private void HideModal(object? sender, RoutedEventArgs e)
        => ModalBackdrop.IsVisible = false;

    // ── Content builders ─────────────────────────────────────────────────────

    private static StackPanel BuildInfoContent() => new()
    {
        Spacing = 6,
        Children =
        {
            new TextBlock { Text = "Vault-Tec firmware update available." },
            new TextBlock
            {
                Text = "Version 4.0.3 includes stability improvements, reduced memory footprint, and a fix for the inventory overflow bug reported in sector 7.",
                TextWrapping = TextWrapping.Wrap,
                Classes      = { "dim" },
                FontSize     = 10,
            },
        },
    };

    private static StackPanel BuildConfirmContent() => new()
    {
        Spacing = 6,
        Children =
        {
            new TextBlock { Text = "INITIATE SELF-DESTRUCT SEQUENCE?", TextWrapping = TextWrapping.Wrap },
            new TextBlock
            {
                Text         = "This action is irreversible. All vault systems will be purged and the main door sealed permanently.",
                TextWrapping = TextWrapping.Wrap,
                Classes      = { "dim" },
                FontSize     = 10,
            },
        },
    };

    private static StackPanel BuildWarningContent() => new()
    {
        Spacing = 6,
        Children =
        {
            new TextBlock { Text = "⚠ RADIATION LEVEL CRITICAL", Classes = { "accent" } },
            new TextBlock
            {
                Text         = "Current exposure: 450 RAD/HR — 4.5× safe threshold. Seek shelter or administer RadAway immediately.",
                TextWrapping = TextWrapping.Wrap,
                Classes      = { "dim" },
                FontSize     = 10,
            },
        },
    };

    private StackPanel BuildAcknowledgeFooter()
    {
        var btn = new Button { Content = "ACKNOWLEDGE" };
        btn.Click += HideModal;
        return new StackPanel { Children = { btn } };
    }

    private StackPanel BuildConfirmFooter()
    {
        var confirm = new Button { Content = "CONFIRM" };
        var cancel  = new Button { Content = "CANCEL"  };
        confirm.Click += HideModal;
        cancel.Click  += HideModal;
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing     = 10,
            Children    = { confirm, cancel },
        };
    }
}
