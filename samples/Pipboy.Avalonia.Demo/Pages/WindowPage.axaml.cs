using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

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
