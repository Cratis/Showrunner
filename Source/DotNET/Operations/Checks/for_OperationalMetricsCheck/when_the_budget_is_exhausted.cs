// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_OperationalMetricsCheck;

public class when_the_budget_is_exhausted : an_operational_metrics_check
{
    async Task Because() => await Run(budget: TimeSpan.Zero);

    [Fact] void should_report_every_source() => _findings.Count.ShouldEqual(4);
    [Fact] void should_not_query_prometheus() => _prometheus.Queries.ShouldBeEmpty();
    [Fact] void should_say_why() => _findings.All(finding => finding.Text.Contains("alert-delivery reserve")).ShouldBeTrue();
}
#endif
