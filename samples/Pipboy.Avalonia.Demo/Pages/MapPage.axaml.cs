using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Pipboy.Avalonia;
using Pipboy.Avalonia.Demo.Data;

namespace Pipboy.Avalonia.Demo.Pages;

/// <summary>
/// World-map demo page for <see cref="PipboyMap"/>.
/// Uses code-behind MVVM-style pattern (no external toolkit required).
/// </summary>
public partial class MapPage : UserControl
{
    public ObservableCollection<MapMarker> Markers { get; } = [];
    public ObservableCollection<MapLine>   Lines   { get; } = [];

    // ── Constructor ────────────────────────────────────────────────────────

    public MapPage()
    {
        InitializeComponent();
        DataContext = this;

        // Load world tiles and bind collections
        TheMap.Tiles   = WorldMapData.CreateTiles();
        TheMap.Markers = Markers;
        TheMap.Lines   = Lines;

        // Status bar: selected tile
        TheMap.PropertyChanged += (_, e) =>
        {
            if (e.Property == PipboyMap.SelectedTileProperty)
                LblSelected.Text = TheMap.SelectedTile is { } t
                    ? $"[ {t.Label ?? t.Name} ]"
                    : "—";
        };

        // Status bar: counts
        Markers.CollectionChanged += (_, _) => LblMarkerCount.Text = Markers.Count.ToString();
        Lines.CollectionChanged   += (_, _) => LblLineCount.Text   = Lines.Count.ToString();

        // MVVM commands
        TheMap.MarkerAddedCommand = new RelayCommand<object>(m =>
        {
            if (m is MapMarker marker && !Markers.Contains(marker))
                Markers.Add(marker);
        });

        TheMap.LineAddedCommand = new RelayCommand<object>(l =>
        {
            if (l is MapLine line && !Lines.Contains(line))
                Lines.Add(line);
        });

        TheMap.TileClickedCommand = new RelayCommand<object>(_ => { });

        // Blink is off by default — matches the initial IsChecked="False" on BtnBlink
        TheMap.MarkersBlinkEnabled = false;
    }

    // ── Zoom / view ────────────────────────────────────────────────────────

    private void OnZoomIn(object? sender, RoutedEventArgs e)    => TheMap.ZoomIn();
    private void OnZoomOut(object? sender, RoutedEventArgs e)   => TheMap.ZoomOut();
    private void OnFitToView(object? sender, RoutedEventArgs e) => TheMap.FitToView();

    // ── Markers ────────────────────────────────────────────────────────────

    private void OnClearMarkers(object? sender, RoutedEventArgs e)
    {
        Markers.Clear();
        TheMap.InvalidateVisual();
    }

    private void OnToggleBlink(object? sender, RoutedEventArgs e)
    {
        TheMap.MarkersBlinkEnabled = BtnBlink?.IsChecked == true;
    }

    private void OnMarkerKindChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (MarkerKindCombo is null || TheMap is null) return;
        if (MarkerKindCombo.SelectedIndex >= 0)
            TheMap.DefaultMarkerKind = (PipboyMapMarkerKind)MarkerKindCombo.SelectedIndex;
    }

    // ── Lines ──────────────────────────────────────────────────────────────

    private void OnToggleDrawLine(object? sender, RoutedEventArgs e)
    {
        TheMap.InteractionMode = BtnDrawLine?.IsChecked == true
            ? PipboyMapInteractionMode.DrawLine
            : PipboyMapInteractionMode.Pan;
    }

    private void OnLineStyleChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (LineStyleCombo is null || TheMap is null) return;
        if (LineStyleCombo.SelectedIndex >= 0)
            TheMap.DefaultLineStyle = (PipboyMapLineStyle)LineStyleCombo.SelectedIndex;
    }

    private void OnToggleThickness(object? sender, RoutedEventArgs e)
    {
        TheMap.DefaultLineIsThick = BtnThick?.IsChecked == true;
    }

    private void OnClearLines(object? sender, RoutedEventArgs e)
    {
        Lines.Clear();
        TheMap.InvalidateVisual();
    }
}

// ── Minimal relay command (no toolkit dependency) ─────────────────────────────

file sealed class RelayCommand<T>(Action<T?> execute, Func<T?, bool>? canExecute = null)
    : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) =>
        canExecute?.Invoke(parameter is T t ? t : default) ?? true;

    public void Execute(object? parameter) =>
        execute(parameter is T t ? t : default);

    public void RaiseCanExecuteChanged() =>
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
