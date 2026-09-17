using Avalonia.Controls;
using Avalonia.Controls.DataGridHierarchical;
using System.Collections.ObjectModel;
using System.Linq;

namespace Pipboy.Avalonia.ProDataGrid.Sample;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var rows = new ObservableCollection<SampleRow>
        {
            new() { Name = "Rad-X",           Category = "Chem",     Status = "Nominal",  Value = 12 },
            new() { Name = "Stimpak",         Category = "Chem",     Status = "Nominal",  Value = 45 },
            new() { Name = "10mm Pistol",     Category = "Weapon",   Status = "Nominal",  Value = 8 },
            new() { Name = "Vault Suit",      Category = "Apparel",  Status = "Damaged",  Value = 3 },
            new() { Name = "Geiger Counter",  Category = "Aid",      Status = "Warning",  Value = 1 },
            new() { Name = "Nuka-Cola",       Category = "Food",     Status = "Nominal",  Value = 22 },
            new() { Name = "Fusion Core",     Category = "Power",    Status = "Critical", Value = 2 },
            new() { Name = "Holotape",        Category = "Misc",     Status = "Nominal",  Value = 6 },
            new() { Name = "RobCo Terminal",  Category = "Tech",     Status = "Nominal",  Value = 1 },
            new() { Name = "Radaway",         Category = "Chem",     Status = "Nominal",  Value = 9 },
        };
        Grid.ItemsSource = rows;

        // Same inventory, regrouped as a two-level tree (category -> item) to demonstrate
        // ProDataGrid's own hierarchical row mode - not the separate, AGPL/Accelerate-licensed
        // Avalonia.Controls.TreeDataGrid package, which this sample deliberately avoids.
        var categoryRoots = rows
            .GroupBy(r => r.Category)
            .Select(group =>
            {
                var category = new SampleTreeItem { Name = group.Key };
                foreach (var row in group)
                {
                    category.Children.Add(new SampleTreeItem
                    {
                        Name = row.Name,
                        Status = row.Status,
                        Value = row.Value,
                    });
                }
                return category;
            })
            .ToList();

        var treeModel = new HierarchicalModel<SampleTreeItem>(new HierarchicalOptions<SampleTreeItem>
        {
            ChildrenSelector = item => item.Children,
        });
        treeModel.SetRoots(categoryRoots);
        treeModel.ExpandAll();

        TreeGrid.HierarchicalModel = treeModel;
    }
}
