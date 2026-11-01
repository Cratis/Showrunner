// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.StageManager.Performances.for_UsageAccumulator;

public class when_the_utc_day_changes : Specification
{
    static readonly DateTimeOffset _lateInTheDay = new(2026, 11, 1, 23, 59, 30, TimeSpan.Zero);

    PerformanceStatus _status;

    void Because()
    {
        _status = new();
        UsageAccumulator.Add(_status, new(1, 1000), _lateInTheDay.AddSeconds(-30));
        UsageAccumulator.Add(_status, new(1, 3000), _lateInTheDay);
        UsageAccumulator.Add(_status, new(2, 500), _lateInTheDay.AddSeconds(60));
    }

    [Fact] void should_report_the_completed_day() => _status.LastPeriodUsage.Day.ShouldEqual("2026-11-01");
    [Fact] void should_keep_what_the_completed_day_used() => _status.LastPeriodUsage.CpuCoreSeconds.ShouldEqual(30d);
    [Fact] void should_keep_the_peak_of_the_completed_day() => _status.LastPeriodUsage.PeakMemoryBytes.ShouldEqual(3000d);
    [Fact] void should_start_the_new_day() => _status.CurrentPeriodUsage.Day.ShouldEqual("2026-11-02");
    [Fact] void should_count_only_the_new_day_in_it() => _status.CurrentPeriodUsage.SampleCount.ShouldEqual(1L);
    [Fact] void should_keep_accumulating_the_total() => _status.ResourceUsage.SampleCount.ShouldEqual(3L);
}
#endif
