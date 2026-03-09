using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace Pipboy.Avalonia.Demo;

public partial class MainView : UserControl
{
    private TextBlock? _breadcrumb;

    public MainView()
    {
        InitializeComponent();

        // After the NavTabControl template is applied, cache the breadcrumb TextBlock
        MainNav.TemplateApplied += (_, e) =>
        {
            _breadcrumb = e.NameScope.Find<TextBlock>("BreadcrumbTitle");
            UpdateBreadcrumb();
        };

        MainNav.SelectionChanged += (_, _) => UpdateBreadcrumb();
    }

    private void UpdateBreadcrumb()
    {
        if (_breadcrumb is null) return;
        _breadcrumb.Text = MainNav.SelectedItem is TabItem tab
            ? tab.Header?.ToString() ?? string.Empty
            : string.Empty;
    }
}
