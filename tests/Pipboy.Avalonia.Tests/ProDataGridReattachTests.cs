using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Pipboy.Avalonia.Tests;

/// <summary>
/// Reproduces the exact shape of a reported Peek bug: a Pipboy-themed DataGrid's horizontal
/// gridlines disappear the first time its hosting view is navigated away from and back to.
/// Peek's shell caches navigated-away views (AsyncNavigation's ContentRegion defaults to
/// caching) and swaps ContentControl.Content to a *different* view and back - not to null and
/// back - so this mirrors that exactly rather than a bare detach/reattach.
/// </summary>
public class ProDataGridReattachTests
{
    private sealed class Item(string name)
    {
        public string Name { get; } = name;
    }

    [AvaloniaFact]
    public void HorizontalGridLines_Survive_Switching_To_Another_View_And_Back()
    {
        // Scoped to this test rather than registered globally in TestAppBuilder: PipboyTheme
        // subscribes to the process-wide PipboyThemeManager.Instance.ThemeColorChanged
        // singleton for as long as it's alive, and nothing else in this assembly tears down
        // Application.Current between tests. A global registration leaked that subscription
        // (holding UI-thread-affine SolidColorBrush fields) into plain [Fact] tests elsewhere
        // that call SetPrimaryColor from a non-Avalonia thread.
        var pipboyTheme = new global::Pipboy.Avalonia.PipboyTheme();
        var proDataGridTheme = new global::Pipboy.Avalonia.ProDataGrid.PipboyProDataGridTheme();
        Application.Current!.Styles.Add(pipboyTheme);
        Application.Current.Styles.Add(proDataGridTheme);

        try
        {
            var items = new ObservableCollection<Item>(
                Enumerable.Range(1, 40).Select(i => new Item($"Row {i}")));

            var grid = new DataGrid
            {
                ItemsSource = items,
                AutoGenerateColumns = false,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
                Height = 200, // Small viewport relative to 40 rows - forces virtualization/recycling.
            };
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = "Name",
                Binding = new global::Avalonia.Data.Binding(nameof(Item.Name)),
            });
            // Mirrors Peek's own XAML exactly: a direct per-instance DynamicResource binding on
            // the brush, not just whatever the theme's ControlTheme Setter provides.
            grid.Bind(DataGrid.HorizontalGridLinesBrushProperty, new DynamicResourceExtension("DataGridGridLinesBrush"));

            var host = new ContentControl { Content = grid };
            var otherView = new TextBlock { Text = "Some other page" };

            var window = new Window { Width = 800, Height = 600, Content = host };
            window.Show();
            PumpLayout(window, grid);

            AssertAllBottomGridLinesVisible(grid);

            // Navigate away: the region shows a different view, the DataGrid is cached (not
            // destroyed) but detached from the visual tree.
            host.Content = otherView;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            // Navigate back: the same cached DataGrid instance is reattached.
            host.Content = grid;
            Dispatcher.UIThread.RunJobs();
            PumpLayout(window, grid);

            AssertAllBottomGridLinesVisible(grid);

            window.Close();
        }
        finally
        {
            Application.Current.Styles.Remove(proDataGridTheme);
            Application.Current.Styles.Remove(pipboyTheme);
            pipboyTheme.Dispose();
        }
    }

    private static void AssertAllBottomGridLinesVisible(DataGrid grid)
    {
        var rows = grid.GetVisualDescendants().OfType<DataGridRow>().ToList();
        Assert.NotEmpty(rows);

        foreach (var row in rows)
        {
            var bottomGridLine = row.GetVisualDescendants()
                .OfType<Rectangle>()
                .FirstOrDefault(r => r.Name == "PART_BottomGridLine");

            Assert.NotNull(bottomGridLine);
            Assert.True(bottomGridLine!.IsVisible, $"Row '{row.DataContext}' bottom gridline should be visible.");
        }
    }

    private static void PumpLayout(Window window, DataGrid grid)
    {
        Dispatcher.UIThread.RunJobs();
        window.ApplyTemplate();
        window.UpdateLayout();
        grid.ApplyTemplate();
        grid.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        grid.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }
}
