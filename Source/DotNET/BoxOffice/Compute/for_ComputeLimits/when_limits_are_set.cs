// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.for_ComputeLimits;

public class when_limits_are_set : a_tenant_read_model<ComputeLimits>
{
    async Task Because() => await Happened(
        new ComputeBudgetSet("tokens", 800),
        new ComputeConcurrencyLimitSet("agent-runs", 4),
        new ComputeBudgetSet("tokens", 600),
        new ComputeConcurrencyLimitSet("tokens", 2));

    [Fact] void should_keep_the_latest_budget() => _scenario.Instance.For("tokens").Budget.ShouldEqual(new MeterQuantity(600));
    [Fact] void should_keep_the_limit_of_each_meter() => _scenario.Instance.For("agent-runs").ConcurrencyLimit.ShouldEqual(4);
    [Fact] void should_keep_both_limits_of_a_meter() => _scenario.Instance.For("tokens").ConcurrencyLimit.ShouldEqual(2);
    [Fact] void should_not_limit_what_nobody_limited() => _scenario.Instance.For("storage").Budget.ShouldBeNull();
}
#endif
