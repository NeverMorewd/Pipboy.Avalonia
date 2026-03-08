using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Pipboy.Avalonia.Demo.Pages;

public partial class MenuPage : UserControl
{
    public MenuPage()
    {
        InitializeComponent();
    }

    private void OnMenuFlyoutBtnClick(object? sender, RoutedEventArgs e)
    {
        MenuStatusText.Text = "Menu opened — select an item or click outside to close.";
    }

    private void OnMenuItemClick(object? sender, RoutedEventArgs e)
    {
        if (sender is MenuItem item)
            MenuStatusText.Text = $"Selected: {item.Header}";
        MenuFlyoutBtn.Flyout?.Hide();
    }
}
