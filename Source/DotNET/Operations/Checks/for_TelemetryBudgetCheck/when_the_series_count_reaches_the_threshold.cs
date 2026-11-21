// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_TelemetryBudgetCheck;

public class when_the_series_count_reaches_the_threshold : a_telemetry_budget_check
{
    int _atTheThreshold;
    int _overTheThreshold;
    int _afterRaisingTheThreshold;

    async Task Because()
    {
        _prometheus.Vectors[SeriesQuery] = [FakePrometheusReader.Sample(350000)];
        await Run();
        _atTheThreshold = _findings.Count;
        _prometheus.Vectors[SeriesQuery] = [FakePrometheusReader.Sample(350001)];
        await Run();
        _overTheThreshold = _findings.Count;
        _options = new TelemetryBudgetOptions { SeriesThreshold = 400000 };
        await Run();
        _afterRaisingTheThreshold = _findings.Count;
    }

    [Fact] void should_not_report_exactly_the_threshold() => _atTheThreshold.ShouldEqual(0);
    [Fact] void should_report_above_it() => _overTheThreshold.ShouldEqual(1);
    [Fact] void should_use_the_configured_threshold() => _afterRaisingTheThreshold.ShouldEqual(0);
}
#endif
