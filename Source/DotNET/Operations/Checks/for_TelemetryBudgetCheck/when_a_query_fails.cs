// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_TelemetryBudgetCheck;

public class when_a_query_fails : a_telemetry_budget_check
{
    void Establish()
    {
        _prometheus.FailingExpressions.Add(CollectorQuery);
        _prometheus.Vectors[LokiQuery] = [FakePrometheusReader.Sample(1, ("reason", "rate_limited"))];
    }

    async Task Because() => await Run();

    [Fact] void should_report_that_the_budgets_cannot_be_read() => Keys.ShouldContain("telemetry:api");
    [Fact] void should_say_which_source_failed() => Message("telemetry:api").ShouldContain("collector:");
    [Fact] void should_not_hide_what_the_other_queries_found() => Keys.Any(key => key.StartsWith("telemetry:loki:")).ShouldBeTrue();
}
#endif
