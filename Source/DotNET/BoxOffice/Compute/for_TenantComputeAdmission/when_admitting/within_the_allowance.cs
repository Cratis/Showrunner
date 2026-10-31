// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.given;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.when_admitting;

public class within_the_allowance : a_tenant_admitting_compute
{
    AdmissionDecision _decision;
    IReadOnlyList<object> _facts;

    async Task Because()
    {
        Allow(Tokens, 1000);
        Measured(Tenants.Today, Tokens, 999);
        _decision = await _admission.Admit(Tokens);
        _facts = await TenantFacts();
    }

    [Fact] void should_admit_the_work() => _decision.IsAdmitted.ShouldBeTrue();
    [Fact] void should_not_track_work_of_a_meter_without_a_concurrency_limit() => _decision.Work.ShouldBeNull();
    [Fact] void should_record_nothing() => _facts.ShouldBeEmpty();
}
#endif
