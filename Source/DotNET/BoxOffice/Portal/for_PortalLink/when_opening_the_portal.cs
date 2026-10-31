// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Seats;

namespace Cratis.Showrunner.BoxOffice.Portal.for_PortalLink;

public class when_opening_the_portal : Specification
{
    static readonly Guid Offer = Guid.NewGuid();

    ICustomerPortal _portal;
    ICurrentSeatHolder _holder;
    PortalLink _link;

    void Establish()
    {
        _portal = Substitute.For<ICustomerPortal>();
        _holder = Substitute.For<ICurrentSeatHolder>();
        _holder.Current.Returns(SeatHolder.Of(Tenants.Bo));
        _portal.LinkFor(PortalIntent.Try, Offer, SeatHolder.Of(Tenants.Bo)).Returns("https://portal.example.com/?signed");
    }

    void Because() => _link = PortalLink.LinkToPortal(PortalIntent.Try, Offer, _portal, _holder);

    [Fact] void should_link_to_the_portal_for_what_the_signed_in_user_wants() => _link.Url.ShouldEqual("https://portal.example.com/?signed");
}
#endif
