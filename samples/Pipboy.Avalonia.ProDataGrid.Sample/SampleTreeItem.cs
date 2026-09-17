using System.Collections.ObjectModel;

namespace Pipboy.Avalonia.ProDataGrid.Sample;

public sealed class SampleTreeItem
{
    public required string Name { get; init; }
    public string? Status { get; init; }
    public int? Value { get; init; }
    public ObservableCollection<SampleTreeItem> Children { get; } = [];
}
