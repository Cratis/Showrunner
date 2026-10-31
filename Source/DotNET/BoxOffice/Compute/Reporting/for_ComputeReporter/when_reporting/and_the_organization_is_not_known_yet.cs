// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Compute.Reporting.for_ComputeReporter.given;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.Reporting.for_ComputeReporter.when_reporting;

public class and_the_organization_is_not_known_yet : a_compute_reporter
{
    Exception _exception;

    async Task Because()
    {
        var context = ContextFor(2);
        _exception = await Catch.Exception(() => _reporter.Reached(new ComputeAllowanceReached("agent-runs", new(Tenants.Today), AllowancePeriod.Day, OverageBehavior.Block), context, DeliveryOf(context)));
        await ReadOutbox();
    }

    [Fact] void should_fail_so_the_report_is_retried() => _exception.ShouldBeOfExactType<ComputeOrganizationNotKnown>();
    [Fact] void should_tell_the_operator_nothing() => _outbox.ShouldBeEmpty();
}
#endif
