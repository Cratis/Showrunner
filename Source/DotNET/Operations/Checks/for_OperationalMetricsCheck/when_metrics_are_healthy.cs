// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_OperationalMetricsCheck;

public class when_metrics_are_healthy : an_operational_metrics_check
{
    async Task Because() => await Run();

    [Fact] void should_find_nothing() => _findings.ShouldBeEmpty();
    [Fact] void should_ask_exactly_the_four_platform_vectors() => _prometheus.Queries.Count.ShouldEqual(4);
    [Fact] void should_give_every_request_a_short_timeout() => _prometheus.Queries.All(query => query.Timeout > TimeSpan.Zero && query.Timeout <= TimeSpan.FromSeconds(2)).ShouldBeTrue();
    [Fact] void should_not_discover_metric_names() => _prometheus.Queries.Any(query => query.Expression.Contains("__name__")).ShouldBeFalse();
}
#endif
