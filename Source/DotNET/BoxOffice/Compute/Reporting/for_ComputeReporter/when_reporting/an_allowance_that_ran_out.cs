// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Compute.Reporting.for_ComputeReporter.given;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.Reporting.for_ComputeReporter.when_reporting;

public class an_allowance_that_ran_out : a_compute_reporter
{
    AllowanceExhausted _reported;

    async Task Because()
    {
        await OrganizationIsKnown();
        var context = ContextFor(2);
        await _reporter.Reached(new ComputeAllowanceReached("agent-runs", new(Tenants.Today), AllowancePeriod.Month, OverageBehavior.Park), context, DeliveryOf(context));
        await ReadOutbox();
        _reported = _outbox.Single().Content as AllowanceExhausted;
    }

    [Fact] void should_tell_the_operator_the_allowance_ran_out() => _reported.ShouldNotBeNull();
    [Fact] void should_name_the_namespace_as_tenant() => _reported.TenantId.ShouldEqual(Tenants.Tenant);
    [Fact] void should_name_the_meter() => _reported.Meter.ShouldEqual(new MeterId("agent-runs"));
    [Fact] void should_name_the_day() => _reported.Day.ShouldEqual(new ComputeDay(Tenants.Today));
    [Fact] void should_name_what_happens_to_work_beyond_it() => _reported.Overage.ShouldEqual(OverageBehavior.Park);
    [Fact] void should_tell_it_under_the_operators_organization() => _outbox.Single().Context.EventSourceId.Value.ShouldEqual(Tenants.Organization.Value.ToString("D"));
}
#endif
