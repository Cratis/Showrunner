// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.given;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.when_admitting;

public class and_a_concurrency_limit_is_set_for_a_meter_that_has_none : a_tenant_admitting_compute
{
    AdmissionDecision _decision;
    IReadOnlyList<object> _facts;

    async Task Because()
    {
        Allow(Tokens, 1000);
        await Maintainers(new ComputeConcurrencyLimitSet(Tokens, 1));
        _decision = await _admission.Admit(Tokens);
        _facts = await TenantFacts();
    }

    [Fact] void should_admit_the_work() => _decision.IsAdmitted.ShouldBeTrue();
    [Fact] void should_not_record_the_work() => _facts.OfType<ComputeWorkStarted>().ShouldBeEmpty();
}
#endif
