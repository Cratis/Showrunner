// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Compute.Reporting.for_ComputeReporter.given;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.Reporting.for_ComputeReporter.when_reporting;

public class the_same_delivery_twice : a_compute_reporter
{
    async Task Because()
    {
        await OrganizationIsKnown();
        var context = ContextFor(2);
        var fact = new ComputeAllowanceReached("agent-runs", new(Tenants.Today), AllowancePeriod.Day, OverageBehavior.Block);

        // The delivery is repeated, as when the reactor's partition is recovered.
        await _reporter.Reached(fact, context, DeliveryOf(context));
        await _reporter.Reached(fact, context, DeliveryOf(context));
        await ReadOutbox();
    }

    [Fact] void should_tell_the_operator_once() => _outbox.Count.ShouldEqual(1);
}
#endif
