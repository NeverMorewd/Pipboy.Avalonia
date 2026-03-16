namespace Pipboy.Avalonia;

/// <summary>Represents a single dot in a <see cref="RatedAttribute"/>.</summary>
public sealed class DotItem
{
    /// <summary>Gets whether this dot is filled (active).</summary>
    public bool IsFilled { get; }

    internal DotItem(bool isFilled) => IsFilled = isFilled;
}
