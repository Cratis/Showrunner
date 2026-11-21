// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_OperationalMetricsCheck;

public class when_a_source_cannot_be_read : an_operational_metrics_check
{
    void Establish() => _prometheus.FailingExpressions.Add(CollectorTargets);

    async Task Because() => await Run();

    [Fact] void should_not_grant_health_to_the_unreadable_source() => Keys.ShouldContain("metrics:collector-targets:api");
    [Fact] void should_read_the_other_sources_anyway() => _prometheus.Queries.Count.ShouldEqual(4);
    [Fact] void should_report_nothing_else() => _findings.Count.ShouldEqual(1);
}
#endif
