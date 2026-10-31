// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.given;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.when_admitting;

public class and_the_allowance_is_used_up : a_tenant_admitting_compute
{
    AdmissionDecision _decision;
    IReadOnlyList<object> _facts;

    async Task Because()
    {
        Allow(Tokens, 1000);
        Measured(Tenants.Today, Tokens, 1000);
        _decision = await _admission.Admit(Tokens);
        _facts = await TenantFacts();
    }

    [Fact] void should_refuse_because_the_allowance_is_exhausted() => _decision.Outcome.ShouldEqual(AdmissionOutcome.AllowanceExhausted);
    [Fact] void should_not_say_when_to_try_again() => _decision.ParkedUntil.ShouldBeNull();
    [Fact] void should_record_that_the_allowance_ran_out() => _facts.Single().ShouldEqual(new ComputeAllowanceReached(Tokens, new(Tenants.Today), AllowancePeriod.Day, OverageBehavior.Block));
}
#endif
