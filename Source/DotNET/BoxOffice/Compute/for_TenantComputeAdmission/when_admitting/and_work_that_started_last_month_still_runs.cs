// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.given;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.when_admitting;

public class and_work_that_started_last_month_still_runs : a_tenant_admitting_compute
{
    AdmissionDecision _decision;

    async Task Because()
    {
        Allow(AgentRuns, 100);
        await Maintainers(new ComputeConcurrencyLimitSet(AgentRuns, 1));

        // It is shortly after midnight on the 1st: the work started in the last minutes of the month before.
        var shortlyAfterMidnight = new Clock(new DateTimeOffset(2026, 11, 1, 0, 5, 0, TimeSpan.Zero));
        await _eventStore.EventLog.Append(
            new Chronicle.Events.EventSourceId($"compute-{AgentRuns.Value}-2026-10"),
            new ComputeWorkStarted(AgentRuns, ComputeWorkId.New(), new DateTimeOffset(2026, 11, 1, 12, 0, 0, TimeSpan.Zero)));
        _decision = await CreateAdmission(shortlyAfterMidnight).Admit(AgentRuns);
    }

    [Fact] void should_still_count_it_against_the_limit() => _decision.Outcome.ShouldEqual(AdmissionOutcome.AtConcurrencyLimit);
}
#endif
