// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Seats.Reporting.for_SeatReporter.given;

namespace Cratis.Showrunner.BoxOffice.Seats.Reporting.for_SeatReporter.when_reporting;

public class a_seat_that_is_assigned : a_seat_reporter
{
    SeatTaken _reported;

    async Task Because()
    {
        await OrganizationIsKnown();
        var context = ContextFor(2);
        await _reporter.Assigned(new SeatAssigned(Tenants.Ada), context, DeliveryOf(context));
        await ReadOutbox();
        _reported = _outbox.Single().Content as SeatTaken;
    }

    [Fact] void should_tell_the_operator_the_seat_is_taken() => _reported.ShouldNotBeNull();
    [Fact] void should_name_the_namespace_as_tenant() => _reported.TenantId.ShouldEqual(Tenants.Tenant);
    [Fact] void should_name_who_took_it() => _reported.Holder.ShouldEqual(Tenants.Ada);
    [Fact] void should_tell_it_under_the_operators_organization() => _outbox.Single().Context.EventSourceId.Value.ShouldEqual(Tenants.Organization.Value.ToString("D"));
}
#endif
