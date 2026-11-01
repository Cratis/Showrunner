// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.StageManager.Performances.for_UsageAccumulator;

public class when_the_metrics_are_not_available : Specification
{
    PerformanceStatus _status;

    void Because()
    {
        _status = new();
        UsageAccumulator.Add(_status, null, new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero));
    }

    [Fact] void should_not_claim_to_have_measured() => _status.ResourceUsage.Measured.ShouldBeFalse();
    [Fact] void should_count_no_samples() => _status.ResourceUsage.SampleCount.ShouldEqual(0L);
    [Fact] void should_report_no_cpu() => _status.ResourceUsage.CpuCoreSeconds.ShouldEqual(0d);
}
#endif
