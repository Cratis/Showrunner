// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.given;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.when_admitting;

public class and_the_monthly_allowance_is_used_up_across_the_month : a_tenant_admitting_compute
{
    AdmissionDecision _decision;

    async Task Because()
    {
        Allow(Tokens, 1000, AllowancePeriod.Month, OverageBehavior.Park);

        // The specification runs on the 2nd of the month: the 1st and the 2nd together use the allowance.
        Measured(Tenants.Day(-1), Tokens, 600);
        Measured(Tenants.Today, Tokens, 400);

        // What was used the month before does not count.
        Measured(Tenants.Day(-2), Tokens, 5000);
        _decision = await _admission.Admit(Tokens);
    }

    [Fact] void should_park_the_work() => _decision.Outcome.ShouldEqual(AdmissionOutcome.Parked);
    [Fact] void should_park_it_until_the_first_of_the_next_month() => _decision.ParkedUntil.ShouldEqual(new DateTimeOffset(2026, 12, 1, 0, 0, 0, TimeSpan.Zero));
}
#endif
