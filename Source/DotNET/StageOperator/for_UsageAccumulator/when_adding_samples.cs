// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.given;
using Cratis.Showrunner.StageOperator.Metering;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.for_UsageAccumulator;

public class when_adding_samples : Specification
{
    StageApplicationStatus _first = null!;
    StageApplicationStatus _second = null!;
    StageApplicationStatus _afterOutage = null!;
    StageApplicationStatus _nextDay = null!;

    static readonly ContainerUsage[] _busy =
    [
        new("orders-1", "stage", 0.5, 400_000_000),
        new("orders-1", "auth-proxy", 0.1, 50_000_000),
        new("orders-mongo-0", "mongo", 0.4, 150_000_000)
    ];

    static readonly ContainerUsage[] _quiet = [new("orders-1", "stage", 0.2, 200_000_000)];

    void Because()
    {
        var start = Applications.Now;
        _first = UsageAccumulator.Add(new StageApplicationStatus(), _busy, start);
        _second = UsageAccumulator.Add(_first, _quiet, start.AddSeconds(30));
        _afterOutage = UsageAccumulator.Add(_second, _quiet, start.AddSeconds(30).AddHours(2));
        _nextDay = UsageAccumulator.Add(_second, _quiet, start.AddHours(13));
    }

    [Fact] void should_mark_the_first_sample_as_measured() => Assert.True(_first.ResourceUsage!.Measured);
    [Fact] void should_not_count_cpu_time_before_there_is_a_window() => Assert.Equal(0, _first.ResourceUsage!.CpuCoreSeconds);
    [Fact] void should_sum_the_memory_of_all_containers() => Assert.Equal(600_000_000, _first.ResourceUsage!.PeakMemoryBytes);
    [Fact] void should_integrate_cpu_over_the_time_since_the_last_sample() => Assert.Equal(0.2 * 30, _second.ResourceUsage!.CpuCoreSeconds, 6);
    [Fact] void should_keep_the_peak() => Assert.Equal(600_000_000, _second.ResourceUsage!.PeakMemoryBytes);
    [Fact] void should_average_the_memory() => Assert.Equal(400_000_000, _second.ResourceUsage!.AverageMemoryBytes);
    [Fact] void should_count_the_samples() => Assert.Equal(2, _second.ResourceUsage!.SampleCount);
    [Fact] void should_record_the_window() => Assert.Equal((Reconciling.Rfc3339.Format(Applications.Now), Reconciling.Rfc3339.Format(Applications.Now.AddSeconds(30))), (_second.ResourceUsage!.WindowStart, _second.ResourceUsage.WindowEnd));
    [Fact] void should_not_count_an_outage_as_running_time() => Assert.Equal(_second.ResourceUsage!.CpuCoreSeconds, _afterOutage.ResourceUsage!.CpuCoreSeconds, 6);
    [Fact] void should_accumulate_the_current_day() => Assert.Equal((Applications.Now.UtcDateTime.ToString("yyyy-MM-dd"), 2L), (_second.CurrentDayUsage!.Day, _second.CurrentDayUsage.SampleCount));
    [Fact] void should_have_no_last_period_during_the_first_day() => Assert.Null(_second.LastPeriodUsage);
    [Fact] void should_close_the_day_into_the_last_period() => Assert.Equal((Applications.Now.UtcDateTime.ToString("yyyy-MM-dd"), true, 600_000_000L), (_nextDay.LastPeriodUsage!.Day, _nextDay.LastPeriodUsage.Measured, _nextDay.LastPeriodUsage.PeakMemoryBytes));
    [Fact] void should_start_a_new_day() => Assert.Equal(1, _nextDay.CurrentDayUsage!.SampleCount);
    [Fact] void should_keep_accumulating_the_total_over_the_day_change() => Assert.Equal(3, _nextDay.ResourceUsage!.SampleCount);
}
#endif
