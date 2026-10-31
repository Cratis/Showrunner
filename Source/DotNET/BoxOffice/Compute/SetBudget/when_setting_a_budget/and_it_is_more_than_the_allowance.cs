// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Compute.SetBudget.when_setting_a_budget;

public class and_it_is_more_than_the_allowance : a_compute_command<SetComputeBudget>
{
    void Establish() => Given(new OperatorSetAllowance(Tokens, 1000, AllowancePeriod.Month, OverageBehavior.Block));

    async Task Because() => _result = await _scenario.Execute(new SetComputeBudget(Tokens, 1001));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
    [Fact] void should_append_nothing() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
