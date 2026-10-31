// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.given;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.when_admitting;

public class and_a_second_attempt_is_refused_in_the_same_period : a_tenant_admitting_compute
{
    AdmissionDecision _second;
    IReadOnlyList<object> _facts;

    async Task Because()
    {
        Allow(Tokens, 1000);
        Measured(Tenants.Today, Tokens, 1000);
        await _admission.Admit(Tokens);
        _second = await _admission.Admit(Tokens);
        _facts = await TenantFacts();
    }

    [Fact] void should_refuse_it_too() => _second.Outcome.ShouldEqual(AdmissionOutcome.AllowanceExhausted);
    [Fact] void should_record_the_allowance_running_out_once() => _facts.Count.ShouldEqual(1);
}
#endif
