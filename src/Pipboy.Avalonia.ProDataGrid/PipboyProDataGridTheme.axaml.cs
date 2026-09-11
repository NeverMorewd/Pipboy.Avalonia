using System;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Pipboy.Avalonia.ProDataGrid;

public partial class PipboyProDataGridTheme : Styles
{
    public PipboyProDataGridTheme(IServiceProvider? serviceProvider = null)
    {
        // Unlike PipboyFxTheme's controls (which apply their own ControlThemes directly),
        // this package is a pure resource/ControlTheme dictionary for a third-party control
        // (DataGrid) - nothing else instantiates it, so it must load its own compiled AXAML
        // here or its StyleInclude chain (Index.axaml -> Themes/Pipboy.v2.xaml) never runs
        // and the DataGrid ControlTheme is never registered.
        AvaloniaXamlLoader.Load(serviceProvider, this);
    }
}
