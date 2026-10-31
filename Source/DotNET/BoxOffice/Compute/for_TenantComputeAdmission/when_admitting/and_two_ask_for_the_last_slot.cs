// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.given;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.when_admitting;

public class and_two_ask_for_the_last_slot : a_tenant_admitting_compute
{
    AdmissionDecision[] _decisions;

    async Task Because()
    {
        Allow(AgentRuns, 100);
        await Maintainers(new ComputeConcurrencyLimitSet(AgentRuns, 1));

        // Several instances of the product ask at once, each with its own scope over the same tenant.
        _decisions = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Task.Run(() => CreateAdmission(_clock).Admit(AgentRuns))));
    }

    [Fact] void should_admit_exactly_one() => _decisions.Count(_ => _.IsAdmitted).ShouldEqual(1);
    [Fact] void should_turn_the_others_away_at_the_concurrency_limit() => _decisions.Count(_ => _.Outcome == AdmissionOutcome.AtConcurrencyLimit).ShouldEqual(7);
}
#endif
