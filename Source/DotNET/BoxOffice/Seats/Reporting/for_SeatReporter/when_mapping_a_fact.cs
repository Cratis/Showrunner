// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Seats.Reporting.for_SeatReporter;

public class when_mapping_a_fact_that_is_not_a_seat_fact : Specification
{
    Exception _exception;

    void Because() => _exception = Catch.Exception(() => SeatReporter.PublicEventFor(new TenantOrganizationNothing(), Tenants.Tenant));

    [Fact] void should_refuse_it() => _exception.ShouldBeOfExactType<NotASeatFact>();
}

public class TenantOrganizationNothing;
#endif
