// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.StageManager.Performances.for_UsageAccumulator;

public class when_adding_samples : Specification
{
    static readonly DateTimeOffset _start = new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);

    PerformanceStatus _status;

    void Because()
    {
        _status = new();
        UsageAccumulator.Add(_status, new(2, 1000), _start);
        UsageAccumulator.Add(_status, new(1, 3000), _start.AddSeconds(30));
        UsageAccumulator.Add(_status, new(0.5, 2000), _start.AddSeconds(60));
    }

    [Fact] void should_be_measured() => _status.ResourceUsage.Measured.ShouldBeTrue();
    [Fact] void should_count_the_samples() => _status.ResourceUsage.SampleCount.ShouldEqual(3L);
    [Fact] void should_integrate_cpu_over_the_time_between_samples() => _status.ResourceUsage.CpuCoreSeconds.ShouldEqual(45d);
    [Fact] void should_keep_the_peak_memory() => _status.ResourceUsage.PeakMemoryBytes.ShouldEqual(3000d);
    [Fact] void should_average_the_memory() => _status.ResourceUsage.AverageMemoryBytes.ShouldEqual(2000d);
    [Fact] void should_start_the_window_at_the_first_sample() => _status.ResourceUsage.WindowStart.ShouldEqual(_start);
    [Fact] void should_end_the_window_at_the_last_sample() => _status.ResourceUsage.WindowEnd.ShouldEqual(_start.AddSeconds(60));
    [Fact] void should_track_the_current_day() => _status.CurrentPeriodUsage.Day.ShouldEqual("2026-11-01");
    [Fact] void should_accumulate_the_current_day_too() => _status.CurrentPeriodUsage.CpuCoreSeconds.ShouldEqual(45d);
    [Fact] void should_have_no_completed_day_yet() => _status.LastPeriodUsage.Measured.ShouldBeFalse();
}
#endif
