using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace Pipboy.Avalonia;

/// <summary>
/// A ContentControl that shows animated Pip-Boy bracket indicators ("> ... <")
/// when selected or hovered, like Fallout's menu selection style.
/// <para>
/// Set <see cref="SelectionGroup"/> to a shared string on multiple instances to get
/// RadioButton-style mutual exclusion: clicking any item in the group selects it and
/// automatically deselects all others in the same group.
/// </para>
/// </summary>
public class BracketHighlight : ContentControl
{
    // ── Static group registry (weak references — no memory leaks) ────────────
    // Keyed by group name; each list holds weak refs so GC'd controls are
    // cleaned up lazily on the next group iteration.
    private static readonly Dictionary<string, List<WeakReference<BracketHighlight>>> _groups
        = new(System.StringComparer.Ordinal);

    // ── Dependency properties ─────────────────────────────────────────────────

    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<BracketHighlight, bool>(nameof(IsSelected), defaultValue: false);

    /// <summary>
    /// Shared group name. Controls with the same non-empty group name act like
    /// radio buttons: selecting one automatically deselects the others.
    /// Leave empty (default) for standalone / independent selection.
    /// </summary>
    public static readonly StyledProperty<string> SelectionGroupProperty =
        AvaloniaProperty.Register<BracketHighlight, string>(
            nameof(SelectionGroup), defaultValue: string.Empty);

    // ── Static constructor ────────────────────────────────────────────────────

    static BracketHighlight()
    {
        IsSelectedProperty.Changed.AddClassHandler<BracketHighlight>(
            (x, e) => x.OnIsSelectedChanged(e.NewValue is true));

        SelectionGroupProperty.Changed.AddClassHandler<BracketHighlight>(
            (x, e) => x.OnSelectionGroupChanged(
                (string?)e.OldValue ?? string.Empty,
                (string?)e.NewValue ?? string.Empty));
    }

    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>Gets or sets whether the bracket selection indicators are shown.</summary>
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>
    /// Gets or sets the selection group name.
    /// Controls sharing the same non-empty name form a mutual-exclusion group.
    /// </summary>
    public string SelectionGroup
    {
        get => GetValue(SelectionGroupProperty);
        set => SetValue(SelectionGroupProperty, value);
    }

    // ── Pointer interaction ───────────────────────────────────────────────────

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        // When the control belongs to a group, a left-click selects it.
        // Standalone controls (no group) keep the existing pointer-over-only behaviour.
        if (!string.IsNullOrEmpty(SelectionGroup)
            && e.InitialPressMouseButton == MouseButton.Left)
        {
            IsSelected = true;
            e.Handled = true;
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void OnIsSelectedChanged(bool isSelected)
    {
        PseudoClasses.Set(":selected", isSelected);

        // Propagate mutual exclusion when this item becomes selected.
        if (isSelected)
            DeselectOthersInGroup();
    }

    private void OnSelectionGroupChanged(string oldGroup, string newGroup)
    {
        UnregisterFromGroup(oldGroup);
        RegisterInGroup(newGroup);
    }

    private void RegisterInGroup(string group)
    {
        if (string.IsNullOrEmpty(group)) return;

        if (!_groups.TryGetValue(group, out var list))
        {
            list = [];
            _groups[group] = list;
        }

        // Purge dead references opportunistically.
        list.RemoveAll(r => !r.TryGetTarget(out _));
        list.Add(new WeakReference<BracketHighlight>(this));
    }

    private void UnregisterFromGroup(string group)
    {
        if (string.IsNullOrEmpty(group)) return;
        if (!_groups.TryGetValue(group, out var list)) return;

        list.RemoveAll(r => !r.TryGetTarget(out var t) || ReferenceEquals(t, this));
        if (list.Count == 0) _groups.Remove(group);
    }

    private void DeselectOthersInGroup()
    {
        string group = SelectionGroup;
        if (string.IsNullOrEmpty(group)) return;
        if (!_groups.TryGetValue(group, out var list)) return;

        // Collect first, set outside the iteration to be safe.
        var others = new List<BracketHighlight>(list.Count);
        foreach (var weakRef in list)
        {
            if (weakRef.TryGetTarget(out var item) && !ReferenceEquals(item, this))
                others.Add(item);
        }

        foreach (var other in others)
            other.IsSelected = false;
    }
}
