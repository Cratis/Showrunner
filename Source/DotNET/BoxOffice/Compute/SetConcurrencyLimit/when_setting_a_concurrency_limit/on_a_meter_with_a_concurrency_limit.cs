// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.SetConcurrencyLimit.when_setting_a_concurrency_limit;

public class on_a_meter_with_a_concurrency_limit : a_compute_command<SetComputeConcurrencyLimit>
{
    async Task Because() => _result = await _scenario.Execute(new SetComputeConcurrencyLimit(AgentRuns, 4));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
    [Fact] void should_append_the_limit() => _scenario.AppendedEvents.Select(_ => _.Event.Content).ShouldEqual([new ComputeConcurrencyLimitSet(AgentRuns, 4)]);
    [Fact] async Task should_append_it_to_the_tenants_stream() => await _scenario.ShouldHaveAppendedEvent<SetComputeConcurrencyLimit, ComputeConcurrencyLimitSet>(TenantStream.Id, e => e.Limit == 4);
}
#endif
