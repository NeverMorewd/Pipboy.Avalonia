using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;

namespace Pipboy.Avalonia;

/// <summary>
/// A Pip-Boy styled countdown timer control.
/// <para>
/// Drive it with <see cref="Start"/>, <see cref="Stop"/> and <see cref="Reset"/>, or set
/// <see cref="AutoStart"/> to begin as soon as the control is attached to the visual tree.
/// Subscribe to <see cref="Tick"/> for per-second callbacks and <see cref="Completed"/>
/// for the moment the counter reaches zero.
/// </para>
/// <para>
/// Uses <see cref="DispatcherTimer"/> internally — fully compatible with WASM and AOT builds.
/// </para>
/// </summary>
public class PipboyCountdown : TemplatedControl
{
    // ── Styled properties ────────────────────────────────────────────────────

    /// <summary>Total countdown duration.</summary>
    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<PipboyCountdown, TimeSpan>(
            nameof(Duration), defaultValue: TimeSpan.FromSeconds(60));

    /// <summary>Optional label rendered above the time display.</summary>
    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<PipboyCountdown, string>(nameof(Label), defaultValue: string.Empty);

    /// <summary>
    /// <see cref="TimeSpan.ToString(string)"/> format used to render the remaining time.
    /// Defaults to <c>"mm\\:ss"</c> which produces e.g. <c>04:59</c>.
    /// Use <c>"hh\\:mm\\:ss"</c> for hours.
    /// </summary>
    public static readonly StyledProperty<string> FormatProperty =
        AvaloniaProperty.Register<PipboyCountdown, string>(nameof(Format), defaultValue: @"mm\:ss");

    /// <summary>
    /// When <see langword="true"/> the countdown starts automatically once the control
    /// is attached to the visual tree.
    /// </summary>
    public static readonly StyledProperty<bool> AutoStartProperty =
        AvaloniaProperty.Register<PipboyCountdown, bool>(nameof(AutoStart), defaultValue: false);

    // ── Direct (computed / state) properties ─────────────────────────────────

    /// <summary>Gets the remaining time. Decrements each second while <see cref="IsRunning"/> is true.</summary>
    public static readonly DirectProperty<PipboyCountdown, TimeSpan> RemainingTimeProperty =
        AvaloniaProperty.RegisterDirect<PipboyCountdown, TimeSpan>(
            nameof(RemainingTime), o => o.RemainingTime);

    /// <summary>Gets the formatted string used by the template's time TextBlock.</summary>
    public static readonly DirectProperty<PipboyCountdown, string> DisplayTimeProperty =
        AvaloniaProperty.RegisterDirect<PipboyCountdown, string>(
            nameof(DisplayTime), o => o.DisplayTime);

    /// <summary>
    /// Gets the elapsed seconds as a fraction of <see cref="Duration"/> for the internal
    /// progress bar (0 = full remaining → bar is full; 1 = expired → bar is empty).
    /// </summary>
    public static readonly DirectProperty<PipboyCountdown, double> ElapsedFractionProperty =
        AvaloniaProperty.RegisterDirect<PipboyCountdown, double>(
            nameof(ElapsedFraction), o => o.ElapsedFraction);

    // ── Backing fields ────────────────────────────────────────────────────────

    private TimeSpan _remainingTime;
    private string _displayTime = string.Empty;
    private double _elapsedFraction;
    private DispatcherTimer? _timer;
    private bool _completed;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Raised once per second while the countdown is running.</summary>
    public event EventHandler? Tick;

    /// <summary>Raised when <see cref="RemainingTime"/> reaches <see cref="TimeSpan.Zero"/>.</summary>
    public event EventHandler? Completed;

    // ── Static constructor ────────────────────────────────────────────────────

    static PipboyCountdown()
    {
        DurationProperty.Changed.AddClassHandler<PipboyCountdown>((x, _) => x.OnDurationChanged());
        FormatProperty.Changed.AddClassHandler<PipboyCountdown>((x, _) => x.RefreshDisplayTime());
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    public PipboyCountdown()
    {
        _remainingTime = Duration;
        RefreshDisplayTime();
        RefreshElapsedFraction();
    }

    // ── Public properties ─────────────────────────────────────────────────────

    /// <summary>Gets or sets the total countdown duration.</summary>
    public TimeSpan Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    /// <summary>Gets or sets the optional label displayed above the countdown.</summary>
    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Gets or sets the <see cref="TimeSpan.ToString(string)"/> format string.</summary>
    public string Format
    {
        get => GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    /// <summary>Gets or sets whether the timer starts automatically on visual-tree attachment.</summary>
    public bool AutoStart
    {
        get => GetValue(AutoStartProperty);
        set => SetValue(AutoStartProperty, value);
    }

    /// <summary>Gets the remaining countdown time.</summary>
    public TimeSpan RemainingTime
    {
        get => _remainingTime;
        private set
        {
            SetAndRaise(RemainingTimeProperty, ref _remainingTime, value);
            RefreshDisplayTime();
            RefreshElapsedFraction();
        }
    }

    /// <summary>Gets the formatted remaining time string rendered by the template.</summary>
    public string DisplayTime
    {
        get => _displayTime;
        private set => SetAndRaise(DisplayTimeProperty, ref _displayTime, value);
    }

    /// <summary>
    /// Gets the elapsed fraction (0–1). 0 = full time remaining, 1 = expired.
    /// Used by the progress bar in the template.
    /// </summary>
    public double ElapsedFraction
    {
        get => _elapsedFraction;
        private set => SetAndRaise(ElapsedFractionProperty, ref _elapsedFraction, value);
    }

    /// <summary>Gets whether the countdown is currently running.</summary>
    public bool IsRunning => _timer is not null;

    /// <summary>
    /// Gets whether the countdown has completed (remaining time has reached zero).
    /// Resets to <see langword="false"/> after calling <see cref="Reset"/>.
    /// </summary>
    public bool IsCompleted => _completed;

    // ── Public methods ────────────────────────────────────────────────────────

    /// <summary>Starts the countdown. No-op if already running or already completed.</summary>
    public void Start()
    {
        if (_timer is not null || _completed || _remainingTime <= TimeSpan.Zero) return;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += OnTimerTick;
        _timer.Start();
        PseudoClasses.Set(":running", true);
    }

    /// <summary>Pauses the countdown without resetting <see cref="RemainingTime"/>.</summary>
    public void Stop()
    {
        StopTimer();
        PseudoClasses.Set(":running", false);
    }

    /// <summary>
    /// Resets <see cref="RemainingTime"/> to <see cref="Duration"/> and stops the timer.
    /// Clears the <c>:completed</c> pseudo-class.
    /// </summary>
    public void Reset()
    {
        StopTimer();
        _completed = false;
        PseudoClasses.Set(":running", false);
        PseudoClasses.Set(":completed", false);
        RemainingTime = Duration;
    }

    // ── Visual tree lifetime ──────────────────────────────────────────────────

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (AutoStart) Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        // Pause but don't reset — allows resuming if reattached.
        StopTimer();
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void OnTimerTick(object? sender, EventArgs e)
    {
        TimeSpan next = _remainingTime - TimeSpan.FromSeconds(1);

        if (next <= TimeSpan.Zero)
        {
            RemainingTime = TimeSpan.Zero;
            StopTimer();
            _completed = true;
            PseudoClasses.Set(":running", false);
            PseudoClasses.Set(":completed", true);
            Tick?.Invoke(this, EventArgs.Empty);
            Completed?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            RemainingTime = next;
            Tick?.Invoke(this, EventArgs.Empty);
        }
    }

    private void StopTimer()
    {
        if (_timer is null) return;
        _timer.Stop();
        _timer.Tick -= OnTimerTick;
        _timer = null;
    }

    private void OnDurationChanged()
    {
        // If not yet started, re-sync RemainingTime to the new Duration.
        if (_timer is null && !_completed)
        {
            _completed = false;
            PseudoClasses.Set(":completed", false);
            RemainingTime = Duration;
        }
    }

    private void RefreshDisplayTime()
    {
        try
        {
            DisplayTime = _remainingTime.ToString(Format);
        }
        catch (FormatException)
        {
            // Fall back to a safe default if the user supplies an invalid format.
            DisplayTime = _remainingTime.ToString(@"mm\:ss");
        }
    }

    private void RefreshElapsedFraction()
    {
        double total = Duration.TotalSeconds;
        ElapsedFraction = total > 0
            ? 1.0 - Math.Clamp(_remainingTime.TotalSeconds / total, 0.0, 1.0)
            : 1.0;
    }
}
