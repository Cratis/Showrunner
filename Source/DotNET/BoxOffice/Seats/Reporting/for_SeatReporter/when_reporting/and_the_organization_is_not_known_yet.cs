// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Seats.Reporting.for_SeatReporter.given;

namespace Cratis.Showrunner.BoxOffice.Seats.Reporting.for_SeatReporter.when_reporting;

public class and_the_organization_is_not_known_yet : a_seat_reporter
{
    Exception _exception;

    async Task Because()
    {
        var context = ContextFor(2);
        _exception = await Catch.Exception(() => _reporter.Assigned(new SeatAssigned(Tenants.Ada), context, DeliveryOf(context)));
        await ReadOutbox();
    }

    [Fact] void should_fail_so_the_report_is_retried() => _exception.ShouldBeOfExactType<OrganizationNotKnown>();
    [Fact] void should_tell_the_operator_nothing() => _outbox.ShouldBeEmpty();
}
#endif
