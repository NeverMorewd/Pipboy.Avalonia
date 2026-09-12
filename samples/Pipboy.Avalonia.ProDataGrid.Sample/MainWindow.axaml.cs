using Avalonia.Controls;
using System.Collections.ObjectModel;

namespace Pipboy.Avalonia.ProDataGrid.Sample;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Grid.ItemsSource = new ObservableCollection<SampleRow>
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
    }
}
