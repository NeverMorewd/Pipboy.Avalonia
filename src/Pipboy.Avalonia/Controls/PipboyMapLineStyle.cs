namespace Pipboy.Avalonia;

/// <summary>Visual stroke style for a <see cref="MapLine"/>.</summary>
public enum PipboyMapLineStyle
{
    /// <summary>Continuous solid stroke.</summary>
    Solid,

    /// <summary>Evenly-spaced dashes.</summary>
    Dashed,

    /// <summary>Small dots at regular intervals.</summary>
    Dotted,

    /// <summary>Dashes with an animated offset — the line appears to flow toward the end point.</summary>
    DashedFlow,
}

/// <summary>Map interaction mode — determines what a left-drag gesture does.</summary>
public enum PipboyMapInteractionMode
{
    /// <summary>Default: drag pans the map; right-click / long-press places markers.</summary>
    Pan,

    /// <summary>Left-drag draws a <see cref="MapLine"/> from the press point to the release point.</summary>
    DrawLine,
}
