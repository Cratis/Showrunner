// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.given;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.when_admitting;

public class and_work_is_running_below_the_concurrency_limit : a_tenant_admitting_compute
{
    AdmissionDecision _first;
    AdmissionDecision _second;
    AdmissionDecision _third;

    async Task Because()
    {
        Allow(AgentRuns, 100);
        await Maintainers(new ComputeConcurrencyLimitSet(AgentRuns, 2));
        _first = await _admission.Admit(AgentRuns);
        _second = await _admission.Admit(AgentRuns);
        _third = await _admission.Admit(AgentRuns);
    }

    [Fact] void should_admit_the_first() => _first.IsAdmitted.ShouldBeTrue();
    [Fact] void should_admit_the_second() => _second.IsAdmitted.ShouldBeTrue();
    [Fact] void should_track_each_admitted_unit_of_work() => _second.Work.ShouldNotEqual(_first.Work);
    [Fact] void should_turn_the_third_away_at_the_concurrency_limit() => _third.Outcome.ShouldEqual(AdmissionOutcome.AtConcurrencyLimit);
}
#endif
