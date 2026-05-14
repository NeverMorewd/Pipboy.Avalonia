using Pipboy.Avalonia.Controls;
using Pipboy.Avalonia.Demo.ViewModels;

namespace Pipboy.Avalonia.Demo;

public partial class MainWindow : PipboyWindow
{
    private readonly ColorPickerViewModel _colorPickerVm = new();
    public MainWindow()
    {
        InitializeComponent();
        PART_TriggerButton.DataContext = _colorPickerVm;
    }
}