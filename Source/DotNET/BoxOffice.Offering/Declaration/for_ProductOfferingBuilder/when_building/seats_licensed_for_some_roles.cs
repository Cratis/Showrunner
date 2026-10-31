// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Declaration.for_ProductOfferingBuilder.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_ProductOfferingBuilder.when_building;

public class seats_licensed_for_some_roles : an_offering_being_declared
{
    void Because()
    {
        _builder.Seats(SeatLicensing.PerUser, "member");
        Build();
    }

    [Fact] void should_need_a_seat_for_that_role() => _offering.Seats.RequiresSeat("member").ShouldBeTrue();
    [Fact] void should_not_need_a_seat_for_another_role() => _offering.Seats.RequiresSeat("viewer").ShouldBeFalse();
}
#endif
