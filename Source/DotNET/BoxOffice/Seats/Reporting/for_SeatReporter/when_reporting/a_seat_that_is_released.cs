// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Seats.Reporting.for_SeatReporter.given;

namespace Cratis.Showrunner.BoxOffice.Seats.Reporting.for_SeatReporter.when_reporting;

public class a_seat_that_is_released : a_seat_reporter
{
    SeatReleased _reported;

    async Task Because()
    {
        await OrganizationIsKnown();
        var context = ContextFor(3);
        await _reporter.Unassigned(new SeatUnassigned(Tenants.Ada), context, DeliveryOf(context));
        await ReadOutbox();
        _reported = _outbox.Single().Content as SeatReleased;
    }

    [Fact] void should_tell_the_operator_the_seat_is_released() => _reported.ShouldNotBeNull();
    [Fact] void should_name_who_released_it() => _reported.Holder.ShouldEqual(Tenants.Ada);
}
#endif
