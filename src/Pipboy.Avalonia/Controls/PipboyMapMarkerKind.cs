namespace Pipboy.Avalonia;

/// <summary>
/// Predefined marker icon styles for <see cref="MapMarker"/>.
/// </summary>
public enum PipboyMapMarkerKind
{
    /// <summary>Standard location pin (default).</summary>
    Pin,

    /// <summary>Triangular flag on a pole.</summary>
    Flag,

    /// <summary>Five-pointed star.</summary>
    Star,

    /// <summary>Skull symbol — danger / hostile zone.</summary>
    Skull,

    /// <summary>Diamond / rhombus shape.</summary>
    Diamond,

    /// <summary>Filled circle.</summary>
    Circle,

    /// <summary>Plus / crosshair cross.</summary>
    Cross,

    /// <summary>Exclamation mark inside a triangle — quest / point of interest.</summary>
    Quest,
}
