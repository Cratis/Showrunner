// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Declaration.for_ProductOfferingBuilder.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_ProductOfferingBuilder.when_building;

public class roles_that_need_a_seat_where_seats_are_not_licensed : an_offering_being_declared
{
    void Because()
    {
        _builder.Seats(SeatLicensing.NotLicensed, "admin");
        Build();
    }

    [Fact] void should_refuse_the_offering() => _invalid.Problems.ShouldContain(_ => _.Contains("does not license seats", StringComparison.Ordinal));
}
#endif
