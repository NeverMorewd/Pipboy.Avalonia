using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pipboy.Avalonia.Tests;

public class PipboyTabStripTests
{
    [Fact]
    public void SelectedIndex_Default_IsNegativeOne()
    {
        var strip = new PipboyTabStrip();
        Assert.Equal(-1, strip.SelectedIndex);
    }

    [AvaloniaFact]
    public async Task Items_CanBeAdded()
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var strip = new PipboyTabStrip();

            strip.Items.Add(new PipboyTabStripItem { Content = "STAT" });
            strip.Items.Add(new PipboyTabStripItem { Content = "INV" });

            Assert.Equal(2, strip.ItemCount);
        });
    }

    [AvaloniaFact]
    public void SelectedIndex_CanBeSet()
    {
        var strip = new PipboyTabStrip();
        strip.Items.Add(new PipboyTabStripItem { Content = "STAT" });
        strip.Items.Add(new PipboyTabStripItem { Content = "INV" });
        strip.SelectedIndex = 0;
        Assert.Equal(0, strip.SelectedIndex);
    }
}
