// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Cratis.Showrunner.StageOperator.Reconciling;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.Metering;

/// <summary>
/// Folds samples of pod metrics into the status: the total since the first sample, and the UTC day in progress,
/// which becomes the last period when the day ends.
/// </summary>
public static class UsageAccumulator
{
    /// <summary>The longest gap between samples that is counted as running; a longer one is an outage of the sampler.</summary>
    public static readonly TimeSpan MaximumGap = TimeSpan.FromMinutes(5);

    /// <summary>Adds one sample of all containers of the application to a status.</summary>
    /// <param name="status">The status.</param>
    /// <param name="containers">The usage of every container of every pod of the application.</param>
    /// <param name="now">The time of the sample.</param>
    /// <returns>The status with the sample folded in.</returns>
    public static StageApplicationStatus Add(StageApplicationStatus status, IReadOnlyList<ContainerUsage> containers, DateTimeOffset now)
    {
        var cpu = containers.Sum(c => c.CpuCores);
        var memory = containers.Sum(c => c.MemoryBytes);

        var total = status.ResourceUsage ?? new StageApplicationUsage { WindowStart = Rfc3339.Format(now) };
        var elapsed = ElapsedSince(total.WindowEnd, now);
        var day = status.CurrentDayUsage;
        var today = now.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var last = status.LastPeriodUsage;
        if (day is not null && day.Day != today)
        {
            last = day with { Measured = day.SampleCount > 0 };
            day = null;
        }

        day ??= new StageApplicationPeriodUsage { Day = today };

        var updatedTotal = total with
        {
            Measured = true,
            CpuCoreSeconds = total.CpuCoreSeconds + (cpu * elapsed),
            PeakMemoryBytes = Math.Max(total.PeakMemoryBytes, memory),
            AverageMemoryBytes = Average(total.AverageMemoryBytes, total.SampleCount, memory),
            WindowEnd = Rfc3339.Format(now),
            SampleCount = total.SampleCount + 1
        };

        var dayElapsed = ElapsedSince(total.WindowEnd, now);
        var updatedDay = day with
        {
            Measured = true,
            CpuCoreSeconds = day.CpuCoreSeconds + (cpu * dayElapsed),
            PeakMemoryBytes = Math.Max(day.PeakMemoryBytes, memory),
            AverageMemoryBytes = Average(day.AverageMemoryBytes, day.SampleCount, memory),
            SampleCount = day.SampleCount + 1
        };

        return status with { ResourceUsage = updatedTotal, CurrentDayUsage = updatedDay, LastPeriodUsage = last };
    }

    static double ElapsedSince(string? windowEnd, DateTimeOffset now)
    {
        if (Rfc3339.Parse(windowEnd) is not { } end)
        {
            return 0;
        }

        var gap = now - end;
        if (gap <= TimeSpan.Zero || gap > MaximumGap)
        {
            return 0;
        }

        return gap.TotalSeconds;
    }

    static long Average(long average, long count, long sample) => (long)Math.Round(((average * (double)count) + sample) / (count + 1), MidpointRounding.ToEven);
}
