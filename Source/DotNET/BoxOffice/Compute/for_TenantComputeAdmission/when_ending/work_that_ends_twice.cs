// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.given;

namespace Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.when_ending;

public class work_that_ends_twice : a_tenant_admitting_compute
{
    IEnumerable<object> _stops;

    async Task Because()
    {
        Allow(AgentRuns, 100);
        await Maintainers(new ComputeConcurrencyLimitSet(AgentRuns, 1));
        var running = await _admission.Admit(AgentRuns);
        await _admission.End(running.Work!);
        await _admission.End(running.Work!);
        _stops = (await TenantFacts()).OfType<ComputeWorkStopped>();
    }

    [Fact] void should_record_that_it_ended_once() => _stops.Count().ShouldEqual(1);
}
#endif
