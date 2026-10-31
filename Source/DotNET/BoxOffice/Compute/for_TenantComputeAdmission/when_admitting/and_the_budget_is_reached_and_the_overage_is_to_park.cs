// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.given;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.when_admitting;

public class and_the_budget_is_reached_and_the_overage_is_to_park : a_tenant_admitting_compute
{
    AdmissionDecision _decision;

    async Task Because()
    {
        Allow(Tokens, 1000, overage: OverageBehavior.Park);
        await Maintainers(new ComputeBudgetSet(Tokens, 400));
        Measured(Tenants.Today, Tokens, 450);
        _decision = await _admission.Admit(Tokens);
    }

    [Fact] void should_park_the_work() => _decision.Outcome.ShouldEqual(AdmissionOutcome.Parked);
    [Fact] void should_say_the_budget_parked_it() => _decision.Because.ShouldEqual(AdmissionOutcome.BudgetReached);
    [Fact] void should_park_it_until_the_next_period_starts() => _decision.ParkedUntil.ShouldEqual(new DateTimeOffset(Tenants.Day(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero));
}
#endif
