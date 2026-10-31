// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.given;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.when_admitting;

public class and_the_budget_is_reached : a_tenant_admitting_compute
{
    AdmissionDecision _decision;
    IReadOnlyList<object> _facts;

    async Task Because()
    {
        Allow(Tokens, 1000);
        await Maintainers(new ComputeBudgetSet(Tokens, 400));
        Measured(Tenants.Today, Tokens, 400);
        _decision = await _admission.Admit(Tokens);
        _facts = await TenantFacts();
    }

    [Fact] void should_refuse_because_the_budget_is_reached() => _decision.Outcome.ShouldEqual(AdmissionOutcome.BudgetReached);
    [Fact] void should_not_record_that_the_allowance_ran_out() => _facts.OfType<ComputeAllowanceReached>().ShouldBeEmpty();
}
#endif
