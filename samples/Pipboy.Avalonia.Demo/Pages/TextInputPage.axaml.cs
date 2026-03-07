using Avalonia.Controls;

namespace Pipboy.Avalonia.Demo.Pages;

public partial class TextInputPage : UserControl
{
    private static readonly string[] Locations =
    [
        "Diamond City", "Goodneighbor", "The Institute",
        "Vault 81", "Sanctuary Hills", "The Glowing Sea",
        "Bunker Hill", "Covenant", "Hangman's Alley"
    ];

    public TextInputPage()
    {
        InitializeComponent();
        LocationAutoComplete.ItemsSource = Locations;
    }
}
