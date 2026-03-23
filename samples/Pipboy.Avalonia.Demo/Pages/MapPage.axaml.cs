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

    // ── Constructor ────────────────────────────────────────────────────────

    public MapPage()
    {
        InitializeComponent();
        DataContext = this;

        // Load world tiles
        TheMap.Tiles   = WorldMapData.CreateTiles();
        TheMap.Markers = Markers;

        // Status bar: selected tile
        TheMap.PropertyChanged += (_, e) =>
        {
            if (e.Property == PipboyMap.SelectedTileProperty)
            {
                LblSelected.Text = TheMap.SelectedTile is { } t
                    ? $"[ {t.Label ?? t.Name} ]"
                    : "—";
            }
        };

        // Status bar: marker count
        Markers.CollectionChanged += (_, _) =>
            LblMarkerCount.Text = Markers.Count.ToString();

        // Intercept marker placement from context menu so it lands in our collection
        TheMap.MarkerAddedCommand = new RelayCommand<object>(m =>
        {
            if (m is MapMarker marker && !Markers.Contains(marker))
                Markers.Add(marker);
        });

        // Wire tile-clicked command
        TheMap.TileClickedCommand = new RelayCommand<object>(_ => { /* status bar updates via PropertyChanged */ });
    }

    // ── Toolbar event handlers ─────────────────────────────────────────────

    private void OnZoomIn(object? sender, RoutedEventArgs e)    => TheMap.ZoomIn();
    private void OnZoomOut(object? sender, RoutedEventArgs e)   => TheMap.ZoomOut();
    private void OnFitToView(object? sender, RoutedEventArgs e) => TheMap.FitToView();

    private void OnClearMarkers(object? sender, RoutedEventArgs e)
    {
        Markers.Clear();
        TheMap.InvalidateVisual();
    }

    private void OnMarkerKindChanged(object? sender, SelectionChangedEventArgs e)
    {
        // MarkerKindCombo / TheMap may be null while InitializeComponent is running
        if (MarkerKindCombo is null || TheMap is null) return;
        if (MarkerKindCombo.SelectedIndex >= 0)
            TheMap.DefaultMarkerKind = (PipboyMapMarkerKind)MarkerKindCombo.SelectedIndex;
    }
}

// ── Minimal relay command (no toolkit dependency) ────────────────────────────

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
