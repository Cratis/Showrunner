// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_TelemetryBudgetCheck;

public class when_the_budgets_are_healthy : a_telemetry_budget_check
{
    async Task Because() => await Run();

    [Fact] void should_find_nothing() => _findings.ShouldBeEmpty();
    [Fact] void should_ask_nine_questions() => _prometheus.Queries.Count.ShouldEqual(9);
    [Fact] void should_give_every_request_a_short_timeout() => _prometheus.Queries.All(query => query.Timeout > TimeSpan.Zero && query.Timeout <= TimeSpan.FromSeconds(2)).ShouldBeTrue();
    [Fact] void should_compare_memory_against_the_limit_of_the_same_container_in_the_platform_namespace() =>
        _prometheus.Queries[7].Expression.ShouldContain("on (namespace, pod, container)");
    [Fact] void should_ask_for_the_memory_of_every_container_not_only_telemetry_ones() => _prometheus.Queries[7].Expression.ShouldNotContain("container=~");
}
#endif
