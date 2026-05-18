using Avalonia.Controls;
using Pipboy.Avalonia.Demo.ViewModels;

namespace Pipboy.Avalonia.Demo;

public partial class MainWindow : Window
{
    private readonly ColorPickerViewModel _colorPickerVm = new();
    public MainWindow()
    {
        InitializeComponent();
        PART_TriggerButton.DataContext = _colorPickerVm;
    }
}