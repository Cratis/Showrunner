// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.given;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.when_admitting;

public class and_the_budget_was_changed : a_tenant_admitting_compute
{
    AdmissionDecision _decision;

    async Task Because()
    {
        Allow(Tokens, 1000);
        await Maintainers(new ComputeBudgetSet(Tokens, 400), new ComputeBudgetSet(Tokens, 800));
        Measured(Tenants.Today, Tokens, 500);
        _decision = await _admission.Admit(Tokens);
    }

    [Fact] void should_follow_the_latest_budget() => _decision.IsAdmitted.ShouldBeTrue();
}
#endif
