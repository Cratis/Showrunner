// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Compute.SetBudget.when_setting_a_budget;

public class and_the_meter_is_not_declared : a_compute_command<SetComputeBudget>
{
    void Establish() => Given(new OperatorSetAllowance("storage", 1000, AllowancePeriod.Month, OverageBehavior.Block));

    async Task Because() => _result = await _scenario.Execute(new SetComputeBudget("storage", 10));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
}
#endif
