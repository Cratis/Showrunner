// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.given;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.when_admitting;

public class and_work_never_ended : a_tenant_admitting_compute
{
    AdmissionDecision _beforeTheLeaseEnds;
    AdmissionDecision _afterTheLeaseEnded;

    async Task Because()
    {
        Allow(AgentRuns, 100);
        await Maintainers(new ComputeConcurrencyLimitSet(AgentRuns, 1));
        await _admission.Admit(AgentRuns);
        _beforeTheLeaseEnds = await _admission.Admit(AgentRuns);

        // The product crashed with the work running and nobody ended it.
        _clock.Advance(_options.MaximumWorkDuration + TimeSpan.FromMinutes(1));
        _afterTheLeaseEnded = await _admission.Admit(AgentRuns);
    }

    [Fact] void should_hold_the_slot_while_the_lease_lasts() => _beforeTheLeaseEnds.Outcome.ShouldEqual(AdmissionOutcome.AtConcurrencyLimit);
    [Fact] void should_free_the_slot_when_the_lease_ends() => _afterTheLeaseEnded.IsAdmitted.ShouldBeTrue();
}
#endif
