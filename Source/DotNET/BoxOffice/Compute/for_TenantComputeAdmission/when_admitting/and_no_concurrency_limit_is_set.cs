// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.given;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.when_admitting;

public class and_no_concurrency_limit_is_set : a_tenant_admitting_compute
{
    AdmissionDecision _decision;
    IReadOnlyList<object> _facts;

    async Task Because()
    {
        Allow(AgentRuns, 100);
        _decision = await _admission.Admit(AgentRuns);
        _facts = await TenantFacts();
    }

    [Fact] void should_admit_the_work() => _decision.IsAdmitted.ShouldBeTrue();
    [Fact] void should_not_record_the_work() => _facts.ShouldBeEmpty();
}
#endif
