// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.SetConcurrencyLimit.when_setting_a_concurrency_limit;

public class with_no_slots : a_compute_command<SetComputeConcurrencyLimit>
{
    async Task Because() => _result = await _scenario.Execute(new SetComputeConcurrencyLimit(AgentRuns, 0));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
}
#endif
