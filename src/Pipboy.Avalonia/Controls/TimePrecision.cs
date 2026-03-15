namespace Pipboy.Avalonia;

/// <summary>
/// Controls how precisely a <see cref="PipboyCountdown"/> displays and decrements time.
/// </summary>
public enum TimePrecision
{
    /// <summary>
    /// Displays hours, minutes and seconds — <c>hh:mm:ss</c>.
    /// Timer ticks every <b>1 second</b>. Suitable for long countdowns.
    /// </summary>
    Hours,

    /// <summary>
    /// Displays minutes and seconds — <c>mm:ss</c>.
    /// Timer ticks every <b>1 second</b>. Default mode.
    /// </summary>
    Seconds,

    /// <summary>
    /// Displays minutes, seconds and milliseconds — <c>mm:ss.fff</c>.
    /// Timer ticks every <b>50 ms</b>. Suitable for fast-paced game timers.
    /// </summary>
    Milliseconds,
}
