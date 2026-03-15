using System;
using Xunit;

namespace Pipboy.Avalonia.Tests;

public class PipboyCountdownTests
{
    // ── Precision defaults ────────────────────────────────────────────────────

    [Fact]
    public void Precision_Default_IsSeconds()
    {
        var cd = new PipboyCountdown();
        Assert.Equal(TimePrecision.Seconds, cd.Precision);
    }

    [Theory]
    [InlineData(TimePrecision.Hours,        @"hh\:mm\:ss")]
    [InlineData(TimePrecision.Seconds,      @"mm\:ss")]
    [InlineData(TimePrecision.Milliseconds, @"mm\:ss\.fff")]
    public void GetDefaultFormat_ReturnsCorrectFormat(TimePrecision precision, string expected)
        => Assert.Equal(expected, PipboyCountdown.GetDefaultFormat(precision));

    [Theory]
    [InlineData(TimePrecision.Hours,        1000)]   // 1 s
    [InlineData(TimePrecision.Seconds,      1000)]   // 1 s
    [InlineData(TimePrecision.Milliseconds,   50)]   // 50 ms
    public void GetTickInterval_ReturnsCorrectMs(TimePrecision precision, int expectedMs)
        => Assert.Equal(
            TimeSpan.FromMilliseconds(expectedMs),
            PipboyCountdown.GetTickInterval(precision));

    // ── Precision → Format auto-sync ─────────────────────────────────────────

    [Fact]
    public void Setting_Precision_Hours_UpdatesFormat()
    {
        var cd = new PipboyCountdown { Precision = TimePrecision.Hours };
        Assert.Equal(@"hh\:mm\:ss", cd.Format);
    }

    [Fact]
    public void Setting_Precision_Seconds_UpdatesFormat()
    {
        var cd = new PipboyCountdown { Precision = TimePrecision.Hours };
        cd.Precision = TimePrecision.Seconds;
        Assert.Equal(@"mm\:ss", cd.Format);
    }

    [Fact]
    public void Setting_Precision_Milliseconds_UpdatesFormat()
    {
        var cd = new PipboyCountdown { Precision = TimePrecision.Milliseconds };
        Assert.Equal(@"mm\:ss\.fff", cd.Format);
    }

    [Fact]
    public void Format_CanBeOverridden_AfterPrecision()
    {
        var cd = new PipboyCountdown { Precision = TimePrecision.Milliseconds, Format = @"ss\.ff" };
        Assert.Equal(@"ss\.ff", cd.Format);
    }

    // ── DisplayTime with each precision ──────────────────────────────────────

    [Fact]
    public void DisplayTime_Hours_FormatsCorrectly()
    {
        var cd = new PipboyCountdown
        {
            Precision = TimePrecision.Hours,
            Duration  = TimeSpan.FromHours(1) + TimeSpan.FromMinutes(5),
        };
        Assert.Equal("01:05:00", cd.DisplayTime);
    }

    [Fact]
    public void DisplayTime_Milliseconds_ContainsDot()
    {
        var cd = new PipboyCountdown
        {
            Precision = TimePrecision.Milliseconds,
            Duration  = TimeSpan.FromSeconds(30),
        };
        Assert.Contains(".", cd.DisplayTime);
    }

    [Fact]
    public void DisplayTime_Milliseconds_HasThreeFractionalDigits()
    {
        var cd = new PipboyCountdown
        {
            Precision = TimePrecision.Milliseconds,
            Duration  = TimeSpan.FromSeconds(5),
        };
        int dotIndex = cd.DisplayTime.IndexOf('.');
        Assert.True(dotIndex >= 0, "DisplayTime should contain '.' for Milliseconds precision");
        Assert.Equal(3, cd.DisplayTime[(dotIndex + 1)..].Length);
    }

    // ── Precision preserves Start/Stop/Reset behaviour ────────────────────────

    [Fact]
    public void Start_Works_WithMillisecondsPrecision()
    {
        var cd = new PipboyCountdown { Precision = TimePrecision.Milliseconds, Duration = TimeSpan.FromSeconds(5) };
        cd.Start();
        Assert.True(cd.IsRunning);
        cd.Stop();
        Assert.False(cd.IsRunning);
    }

    [Fact]
    public void Reset_Works_WithMillisecondsPrecision()
    {
        var cd = new PipboyCountdown { Precision = TimePrecision.Milliseconds, Duration = TimeSpan.FromSeconds(5) };
        cd.Start();
        cd.Reset();
        Assert.False(cd.IsRunning);
        Assert.Equal(TimeSpan.FromSeconds(5), cd.RemainingTime);
    }

    [Fact]
    public void ElapsedFraction_IsZero_WithMillisecondsPrecision()
    {
        var cd = new PipboyCountdown { Precision = TimePrecision.Milliseconds, Duration = TimeSpan.FromSeconds(10) };
        Assert.Equal(0.0, cd.ElapsedFraction);
    }

    // ── Initial state ─────────────────────────────────────────────────────────

    [Fact]
    public void Duration_Default_Is60Seconds()
    {
        var cd = new PipboyCountdown();
        Assert.Equal(TimeSpan.FromSeconds(60), cd.Duration);
    }

    [Fact]
    public void RemainingTime_EqualssDuration_Initially()
    {
        var cd = new PipboyCountdown { Duration = TimeSpan.FromSeconds(30) };
        Assert.Equal(cd.Duration, cd.RemainingTime);
    }

    [Fact]
    public void IsRunning_Default_IsFalse()
    {
        var cd = new PipboyCountdown();
        Assert.False(cd.IsRunning);
    }

    [Fact]
    public void AutoStart_Default_IsFalse()
    {
        var cd = new PipboyCountdown();
        Assert.False(cd.AutoStart);
    }

    [Fact]
    public void Label_Default_IsEmpty()
    {
        var cd = new PipboyCountdown();
        Assert.Equal(string.Empty, cd.Label);
    }

    [Fact]
    public void Format_Default_IsMinutesColonSeconds()
    {
        var cd = new PipboyCountdown();
        Assert.Equal(@"mm\:ss", cd.Format);
    }

    // ── DisplayTime formatting ────────────────────────────────────────────────

    [Fact]
    public void DisplayTime_FormatsRemainingTime_WithDefaultFormat()
    {
        var cd = new PipboyCountdown { Duration = TimeSpan.FromSeconds(90) };
        // 90 seconds = 01:30
        Assert.Equal("01:30", cd.DisplayTime);
    }

    [Fact]
    public void DisplayTime_FormatsRemainingTime_WithCustomFormat()
    {
        var cd = new PipboyCountdown
        {
            Duration = TimeSpan.FromHours(1) + TimeSpan.FromMinutes(5),
            Format = @"hh\:mm\:ss"
        };
        Assert.Equal("01:05:00", cd.DisplayTime);
    }

    [Fact]
    public void DisplayTime_UpdatesWhenDurationChanges()
    {
        var cd = new PipboyCountdown { Duration = TimeSpan.FromSeconds(10) };
        Assert.Equal("00:10", cd.DisplayTime);

        cd.Duration = TimeSpan.FromSeconds(45);
        Assert.Equal("00:45", cd.DisplayTime);
    }

    // ── ElapsedFraction ───────────────────────────────────────────────────────

    [Fact]
    public void ElapsedFraction_IsZero_WhenNothingElapsed()
    {
        var cd = new PipboyCountdown { Duration = TimeSpan.FromSeconds(60) };
        Assert.Equal(0.0, cd.ElapsedFraction);
    }

    [Fact]
    public void ElapsedFraction_IsOne_WhenCompleted()
    {
        var cd = new PipboyCountdown { Duration = TimeSpan.FromSeconds(60) };
        // Manually drive RemainingTime to zero via Reset + Duration trick
        cd.Duration = TimeSpan.Zero;
        Assert.InRange(cd.ElapsedFraction, 0.99, 1.0);
    }

    // ── Start / Stop / Reset ──────────────────────────────────────────────────

    [Fact]
    public void Start_SetsIsRunningTrue()
    {
        var cd = new PipboyCountdown { Duration = TimeSpan.FromSeconds(10) };
        cd.Start();
        Assert.True(cd.IsRunning);
        cd.Stop();
    }

    [Fact]
    public void Stop_SetsIsRunningFalse()
    {
        var cd = new PipboyCountdown { Duration = TimeSpan.FromSeconds(10) };
        cd.Start();
        cd.Stop();
        Assert.False(cd.IsRunning);
    }

    [Fact]
    public void Stop_DoesNotResetRemainingTime()
    {
        var cd = new PipboyCountdown { Duration = TimeSpan.FromSeconds(10) };
        cd.Start();
        cd.Stop();
        // RemainingTime is still 10 s (timer hasn't ticked in the test)
        Assert.Equal(TimeSpan.FromSeconds(10), cd.RemainingTime);
    }

    [Fact]
    public void Reset_RestoresRemainingTimeToDuration()
    {
        var cd = new PipboyCountdown { Duration = TimeSpan.FromSeconds(20) };
        cd.Start();
        cd.Stop();
        // Pretend time passed by directly resetting.
        cd.Reset();
        Assert.Equal(TimeSpan.FromSeconds(20), cd.RemainingTime);
    }

    [Fact]
    public void Reset_SetsIsRunningFalse()
    {
        var cd = new PipboyCountdown { Duration = TimeSpan.FromSeconds(10) };
        cd.Start();
        cd.Reset();
        Assert.False(cd.IsRunning);
    }

    [Fact]
    public void Start_IsIdempotent_WhenAlreadyRunning()
    {
        var cd = new PipboyCountdown { Duration = TimeSpan.FromSeconds(10) };
        cd.Start();
        cd.Start(); // second call must not create a second timer
        Assert.True(cd.IsRunning);
        cd.Stop();
    }

    // ── Duration change while idle ────────────────────────────────────────────

    [Fact]
    public void Duration_Change_SyncsRemainingTime_WhenIdle()
    {
        var cd = new PipboyCountdown { Duration = TimeSpan.FromSeconds(30) };
        cd.Duration = TimeSpan.FromSeconds(90);
        Assert.Equal(TimeSpan.FromSeconds(90), cd.RemainingTime);
    }

    // ── Running / Completed state ─────────────────────────────────────────────
    // PseudoClasses is protected in TemplatedControl; test equivalent public properties.

    [Fact]
    public void IsRunning_TrueAfterStart()
    {
        var cd = new PipboyCountdown { Duration = TimeSpan.FromSeconds(10) };
        cd.Start();
        Assert.True(cd.IsRunning);
        cd.Stop();
    }

    [Fact]
    public void IsRunning_FalseAfterStop()
    {
        var cd = new PipboyCountdown { Duration = TimeSpan.FromSeconds(10) };
        cd.Start();
        cd.Stop();
        Assert.False(cd.IsRunning);
    }

    [Fact]
    public void IsCompleted_FalseAfterReset()
    {
        var cd = new PipboyCountdown { Duration = TimeSpan.FromSeconds(10) };
        cd.Start();
        cd.Stop();
        cd.Reset();
        Assert.False(cd.IsCompleted);
    }

    [Fact]
    public void IsCompleted_FalseInitially()
    {
        var cd = new PipboyCountdown { Duration = TimeSpan.FromSeconds(10) };
        Assert.False(cd.IsCompleted);
    }

    // ── Events ────────────────────────────────────────────────────────────────

    [Fact]
    public void Completed_EventWired_CanBeSubscribed()
    {
        bool fired = false;
        var cd = new PipboyCountdown { Duration = TimeSpan.FromSeconds(5) };
        cd.Completed += (_, _) => fired = true;
        // Just verify the subscription doesn't throw; actual firing requires timer tick.
        Assert.False(fired);
    }

    [Fact]
    public void Tick_EventWired_CanBeSubscribed()
    {
        int count = 0;
        var cd = new PipboyCountdown { Duration = TimeSpan.FromSeconds(5) };
        cd.Tick += (_, _) => count++;
        Assert.Equal(0, count);
    }
}
