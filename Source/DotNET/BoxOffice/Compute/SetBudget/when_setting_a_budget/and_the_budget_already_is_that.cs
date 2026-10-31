// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Compute.SetBudget.when_setting_a_budget;

public class and_the_budget_already_is_that : a_compute_command<SetComputeBudget>
{
    void Establish() => Given(
        new OperatorSetAllowance(Tokens, 1000, AllowancePeriod.Month, OverageBehavior.Block),
        new ComputeBudgetSet(Tokens, 800));

    async Task Because() => _result = await _scenario.Execute(new SetComputeBudget(Tokens, 800));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
    [Fact] void should_append_nothing() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
