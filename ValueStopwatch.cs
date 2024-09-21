﻿using System;
using System.Threading;

namespace WinUI_Cube;

/// <summary>
/// A memory efficient version of the System.Diagnostics.Stopwatch class.
/// Because this timer's function is passive, there's no need/way for a
/// stop method. A reset method would be equivalent to calling StartNew().
/// </summary>
internal struct ValueStopwatch
{
    // Set the ratio of timespan ticks to stopwatch ticks.
    private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)System.Diagnostics.Stopwatch.Frequency;
    private long _startTimestamp;
    public bool IsActive => _startTimestamp != 0;
    private ValueStopwatch(long startTimestamp) => _startTimestamp = startTimestamp;
    public static ValueStopwatch StartNew() => new ValueStopwatch(System.Diagnostics.Stopwatch.GetTimestamp());
    public TimeSpan GetElapsedTime()
    {
        // _startTimestamp cannot be zero for an initialized ValueStopwatch.
        if (!IsActive)
            throw new InvalidOperationException($"{nameof(ValueStopwatch)} is uninitialized. Initialize the {nameof(ValueStopwatch)} before using.");

        long end = System.Diagnostics.Stopwatch.GetTimestamp();
        long timestampDelta = end - _startTimestamp;
        long ticks = (long)(TimestampToTicks * timestampDelta);
        return new TimeSpan(ticks);
    }

    /// <summary>
    /// Just an example of another way to do what ValueStopwatch does.
    /// </summary>
    void AlternativeTechniqueInsteadOfUsingValueStopwatch()
    {
        var start = System.Diagnostics.Stopwatch.GetTimestamp();
        Thread.Sleep(1000);
        var end = System.Diagnostics.Stopwatch.GetTimestamp();
        var diff = TimeSpan.FromTicks(end - start);
        System.Diagnostics.Debug.WriteLine($"{diff.Seconds:N6} seconds.");
    }
}
