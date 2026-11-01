// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Adds samples of CPU and memory to the usage in a status: the total, the UTC day that is under way and the last completed day.
/// </summary>
public static class UsageAccumulator
{
    /// <summary>
    /// The longest span one sample stands for. After a longer gap (the operator was down) the gap is not billed as if the CPU had been in use throughout.
    /// </summary>
    public static readonly TimeSpan MaxInterval = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Adds a sample.
    /// </summary>
    /// <param name="status">The status to update.</param>
    /// <param name="sample">The sample, or null when the metrics were not available.</param>
    /// <param name="now">The current time.</param>
    public static void Add(PerformanceStatus status, UsageSample? sample, DateTimeOffset now)
    {
        var day = now.UtcDateTime.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        if (string.IsNullOrEmpty(status.CurrentPeriodUsage.Day))
        {
            status.CurrentPeriodUsage.Day = day;
        }
        else if (!string.Equals(status.CurrentPeriodUsage.Day, day, StringComparison.Ordinal))
        {
            status.LastPeriodUsage = status.CurrentPeriodUsage;
            status.CurrentPeriodUsage = new() { Day = day };
        }

        if (sample is null)
        {
            return;
        }

        var elapsed = status.ResourceUsage.WindowEnd is { } previous ? Clamp(now - previous) : TimeSpan.Zero;
        Accumulate(status.ResourceUsage, sample, elapsed, now);
        Accumulate(status.CurrentPeriodUsage, sample, elapsed, now);
    }

    static TimeSpan Clamp(TimeSpan elapsed) => TimeSpan.FromTicks(Math.Clamp(elapsed.Ticks, 0, MaxInterval.Ticks));

    static void Accumulate(ResourceUsageStatus usage, UsageSample sample, TimeSpan elapsed, DateTimeOffset now)
    {
        usage.CpuCoreSeconds += sample.CpuCores * elapsed.TotalSeconds;
        usage.PeakMemoryBytes = Math.Max(usage.PeakMemoryBytes, sample.MemoryBytes);
        usage.AverageMemoryBytes = ((usage.AverageMemoryBytes * usage.SampleCount) + sample.MemoryBytes) / (usage.SampleCount + 1);
        usage.SampleCount++;
        usage.WindowStart ??= now;
        usage.WindowEnd = now;
        usage.Measured = true;
    }
}
