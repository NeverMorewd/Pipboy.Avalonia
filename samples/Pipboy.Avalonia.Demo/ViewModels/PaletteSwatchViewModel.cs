using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Reactive;
using System.Reactive.Linq;

namespace Pipboy.Avalonia.Demo.ViewModels;

public partial class PaletteSwatchViewModel : ReactiveObject
{
    private readonly ColorPickerViewModel _owner;

    public string Hex { get; }

    public ISolidColorBrush Brush { get; }

    [Reactive]
    private bool _isSelected;

    // Rx Command
    public ReactiveCommand<Unit, Unit> SelectCommand { get; }

    public PaletteSwatchViewModel(string hex, ColorPickerViewModel owner)
    {
        _owner = owner;
        Hex = hex.ToUpperInvariant();
        Brush = new SolidColorBrush(Color.Parse(hex));

        SelectCommand = ReactiveCommand.Create(() =>
        {
            _owner.SelectColorCommand.Execute(Hex).Subscribe();
        });

        _owner.WhenAnyValue(x => x.SelectedColor)
           .Select(color =>
               string.Equals(
                   $"#{color.R:X2}{color.G:X2}{color.B:X2}",
                   Hex,
                   StringComparison.OrdinalIgnoreCase))
           .Subscribe(isSelected =>
           {
               IsSelected = isSelected;
           });
    }
}