using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Reactive;

namespace Pipboy.Avalonia.Demo.ViewModels;

public partial class ColorPickerViewModel : ReactiveObject
{
    private static readonly string[] RawPalette =
    [
        "#39FF14", "#00FF41", "#7FFF00", "#ADFF2F",
        "#00FFFF", "#00BFFF", "#1E90FF", "#6495ED",
        "#FF4500", "#FF6347", "#FF69B4", "#FF1493",
        "#FFD700", "#FFA500", "#FF8C00", "#FFFF00",
        "#EE82EE", "#DA70D6", "#BA55D3", "#9400D3",
        "#FFFFFF", "#A0A0A0", "#505050", "#202020",
    ];

    public ObservableCollection<PaletteSwatchViewModel> Swatches { get; } = [];

    [Reactive]
    private Color _selectedColor = Color.Parse("#39FF14");
    [Reactive]
    private string _hexLabel = "";
    [Reactive]
    private ISolidColorBrush _currentBrush = new SolidColorBrush(Color.Parse("#39FF14"));

    public ReactiveCommand<string, Unit> SelectColorCommand { get; }


    public ColorPickerViewModel()
    {
        foreach (var hex in RawPalette)
            Swatches.Add(new PaletteSwatchViewModel(hex, this));

        SelectColorCommand = ReactiveCommand.Create<string>(hex =>
        {
            SelectedColor = Color.Parse(hex);
        });

        this.WhenAnyValue(x => x.SelectedColor)
            .Subscribe(UpdateSelectionMarkers);

        this.WhenAnyValue(x => x.SelectedColor)
            .Subscribe(PipboyThemeManager.Instance.SetPrimaryColor);

        this.WhenAnyValue(x => x.SelectedColor)
            .Subscribe(color =>
            {
                CurrentBrush = new SolidColorBrush(color);
                HexLabel = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            });

        UpdateSelectionMarkers(SelectedColor);
    }

    private void UpdateSelectionMarkers(Color color)
    {
        var target = $"#{color.R:X2}{color.G:X2}{color.B:X2}".ToUpperInvariant();

        foreach (var swatch in Swatches)
        {
            swatch.IsSelected =
                string.Equals(swatch.Hex, target, StringComparison.OrdinalIgnoreCase);
        }
    }
}