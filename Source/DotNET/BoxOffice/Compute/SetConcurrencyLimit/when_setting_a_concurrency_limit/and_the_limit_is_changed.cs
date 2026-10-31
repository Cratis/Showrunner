// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.SetConcurrencyLimit.when_setting_a_concurrency_limit;

public class and_the_limit_is_changed : a_compute_command<SetComputeConcurrencyLimit>
{
    void Establish() => Given(new ComputeConcurrencyLimitSet(AgentRuns, 4));

    async Task Because() => _result = await _scenario.Execute(new SetComputeConcurrencyLimit(AgentRuns, 2));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
    [Fact] void should_append_the_new_limit() => _scenario.AppendedEvents.Select(_ => _.Event.Content).ShouldEqual([new ComputeConcurrencyLimitSet(AgentRuns, 2)]);
}
#endif
