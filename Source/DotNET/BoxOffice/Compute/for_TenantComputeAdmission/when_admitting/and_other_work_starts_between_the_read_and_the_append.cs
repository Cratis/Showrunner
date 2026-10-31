// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.given;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.when_admitting;

public class and_other_work_starts_between_the_read_and_the_append : a_tenant_admitting_compute
{
    AdmissionDecision _decision;
    IReadOnlyList<object> _facts;

    async Task Because()
    {
        Allow(AgentRuns, 100);
        await Maintainers(new ComputeConcurrencyLimitSet(AgentRuns, 1));

        // Another instance takes the last slot after this one has read that it is free: the clock is read right before the
        // append, which is when the other instance gets in.
        var racing = new RacingClock(_clock, () => _eventStore.EventLog.Append(
            new Chronicle.Events.EventSourceId($"compute-{AgentRuns.Value}-{Tenants.Today:yyyy-MM}"),
            new ComputeWorkStarted(AgentRuns, ComputeWorkId.New(), Tenants.Now.AddHours(1))).GetAwaiter().GetResult());
        _decision = await CreateAdmission(racing).Admit(AgentRuns);
        _facts = await TenantFacts();
    }

    [Fact] void should_turn_the_work_away_instead_of_overfilling_the_limit() => _decision.Outcome.ShouldEqual(AdmissionOutcome.AtConcurrencyLimit);
    [Fact] void should_have_recorded_only_the_other_work() => _facts.OfType<ComputeWorkStarted>().Count().ShouldEqual(1);

    /// <summary>
    /// A clock that lets something happen the second time it is read, which is after the admission read what is running.
    /// </summary>
    /// <param name="inner">The clock.</param>
    /// <param name="onSecondRead">What happens.</param>
    class RacingClock(TimeProvider inner, Action onSecondRead) : TimeProvider
    {
        int _reads;

        public override DateTimeOffset GetUtcNow()
        {
            if (++_reads == 2)
            {
                onSecondRead();
            }

            return inner.GetUtcNow();
        }
    }
}
#endif
