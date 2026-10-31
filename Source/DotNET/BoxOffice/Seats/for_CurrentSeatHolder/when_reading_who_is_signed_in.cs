// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Seats.for_CurrentSeatHolder;

public class when_reading_who_is_signed_in : Specification
{
    SeatHolder _signedIn;
    SeatHolder _anonymous;

    void Because()
    {
        _signedIn = new CurrentSeatHolder(new FixedPrincipal(Tenants.SignedInAs(Tenants.Ada))).Current;
        _anonymous = new CurrentSeatHolder(new FixedPrincipal(new ClaimsPrincipal(new ClaimsIdentity()))).Current;
    }

    [Fact] void should_be_the_person_at_their_identity_provider() => _signedIn.ShouldEqual(SeatHolder.Of(Tenants.Ada));
    [Fact] void should_be_nobody_when_nobody_is_signed_in() => _anonymous.ShouldBeNull();
}
#endif
