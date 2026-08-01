using Avalonia.Controls;
using Pipboy.Avalonia.Demo.ViewModels;

namespace Pipboy.Avalonia.Demo.Pages;

public partial class AvaloniaEditPage : UserControl
{
    public AvaloniaEditPage()
    {
        InitializeComponent();
        DataContext = new AvaloniaEditViewModel();
    }
}