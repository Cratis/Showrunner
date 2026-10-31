// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Compute.SetBudget.when_setting_a_budget;

public class and_it_is_within_the_allowance : a_compute_command<SetComputeBudget>
{
    void Establish() => Given(new OperatorSetAllowance(Tokens, 1000, AllowancePeriod.Month, OverageBehavior.Block));

    async Task Because() => _result = await _scenario.Execute(new SetComputeBudget(Tokens, 800));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
    [Fact] void should_append_the_budget() => _scenario.AppendedEvents.Select(_ => _.Event.Content).ShouldEqual([new ComputeBudgetSet(Tokens, 800)]);
    [Fact] async Task should_append_it_to_the_tenants_stream() => await _scenario.ShouldHaveAppendedEvent<SetComputeBudget, ComputeBudgetSet>(TenantStream.Id, e => e.Quantity == 800);
}
#endif
