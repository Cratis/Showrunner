// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.StageManager.Performances.for_UsageAccumulator;

public class when_the_operator_was_away_between_samples : Specification
{
    static readonly DateTimeOffset _start = new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);

    PerformanceStatus _status;

    void Because()
    {
        _status = new();
        UsageAccumulator.Add(_status, new(1, 1000), _start);
        UsageAccumulator.Add(_status, new(1, 1000), _start.AddHours(3));
    }

    [Fact] void should_not_bill_the_gap_as_if_the_cpu_was_in_use_throughout() => _status.ResourceUsage.CpuCoreSeconds.ShouldEqual(UsageAccumulator.MaxInterval.TotalSeconds);
}
#endif
