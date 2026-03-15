using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace Pipboy.Avalonia;

/// <summary>
/// A horizontal tab-strip navigation control in Pip-Boy style.
/// Supports D-Pad Left/Right keyboard navigation for gamepad use.
/// </summary>
public class PipboyTabStrip : ListBox
{
    static PipboyTabStrip()
    {
        SelectionModeProperty.OverrideDefaultValue<PipboyTabStrip>(SelectionMode.Single);
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new PipboyTabStripItem();

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        recycleKey = null;
        return item is not PipboyTabStripItem;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        int count = ItemCount;
        if (count == 0) { base.OnKeyDown(e); return; }

        if (e.Key == Key.Left)
        {
            int prev = SelectedIndex > 0 ? SelectedIndex - 1 : count - 1;
            SelectedIndex = prev;
            e.Handled = true;
        }
        else if (e.Key == Key.Right)
        {
            int next = SelectedIndex < count - 1 ? SelectedIndex + 1 : 0;
            SelectedIndex = next;
            e.Handled = true;
        }
        else
        {
            base.OnKeyDown(e);
        }
    }
}
