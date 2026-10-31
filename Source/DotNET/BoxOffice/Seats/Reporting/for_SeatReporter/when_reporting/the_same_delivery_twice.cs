// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Seats.Reporting.for_SeatReporter.given;

namespace Cratis.Showrunner.BoxOffice.Seats.Reporting.for_SeatReporter.when_reporting;

public class the_same_delivery_twice : a_seat_reporter
{
    async Task Because()
    {
        await OrganizationIsKnown();
        var context = ContextFor(2);

        // The delivery is repeated, as when the reactor's partition is recovered.
        await _reporter.Assigned(new SeatAssigned(Tenants.Ada), context, DeliveryOf(context));
        await _reporter.Assigned(new SeatAssigned(Tenants.Ada), context, DeliveryOf(context));
        await ReadOutbox();
    }

    [Fact] void should_tell_the_operator_once() => _outbox.Count.ShouldEqual(1);
}
#endif
