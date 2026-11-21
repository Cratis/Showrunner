// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_TelemetryBudgetCheck;

public class when_the_check_is_late : a_telemetry_budget_check
{
    IReadOnlyCollection<Alerts.Finding> _exhausted;
    TimeSpan _shortest;
    int _exhaustedQueries;

    async Task Because()
    {
        await Run(budget: TimeSpan.Zero);
        _exhausted = _findings;
        _exhaustedQueries = _prometheus.Queries.Count;

        _prometheus.Queries.Clear();
        await Run(budget: TimeSpan.FromMilliseconds(500));
        _shortest = _prometheus.Queries.Min(query => query.Timeout);
    }

    [Fact] void should_report_an_exhausted_budget_instead_of_skipping_silently() => _exhausted.Single().Key.ShouldEqual("telemetry:api");
    [Fact] void should_say_why() => _exhausted.Single().Text.ShouldContain("alert-delivery reserve");
    [Fact] void should_never_ask_for_more_time_than_is_left_to_collect() => (_shortest <= TimeSpan.FromMilliseconds(500)).ShouldBeTrue();
    [Fact] void should_not_query_prometheus_once_the_budget_is_gone() => _exhaustedQueries.ShouldEqual(0);
}
#endif
