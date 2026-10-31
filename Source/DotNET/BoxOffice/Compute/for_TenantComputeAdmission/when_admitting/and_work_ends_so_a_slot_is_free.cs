// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.given;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.when_admitting;

public class and_work_ends_so_a_slot_is_free : a_tenant_admitting_compute
{
    AdmissionDecision _atTheLimit;
    AdmissionDecision _afterItEnded;

    async Task Because()
    {
        Allow(AgentRuns, 100);
        await Maintainers(new ComputeConcurrencyLimitSet(AgentRuns, 1));
        var running = await _admission.Admit(AgentRuns);
        _atTheLimit = await _admission.Admit(AgentRuns);
        await _admission.End(running.Work!);
        _afterItEnded = await _admission.Admit(AgentRuns);
    }

    [Fact] void should_turn_work_away_while_the_slot_is_held() => _atTheLimit.Outcome.ShouldEqual(AdmissionOutcome.AtConcurrencyLimit);
    [Fact] void should_admit_work_once_it_ended() => _afterItEnded.IsAdmitted.ShouldBeTrue();
}
#endif
