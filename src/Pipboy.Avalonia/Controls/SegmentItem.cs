namespace Pipboy.Avalonia;

/// <summary>Represents a single segment in a <see cref="SegmentedBar"/>.</summary>
public sealed class SegmentItem
{
    /// <summary>Gets whether this segment is filled (active).</summary>
    public bool IsFilled { get; }

    internal SegmentItem(bool isFilled) => IsFilled = isFilled;
}
