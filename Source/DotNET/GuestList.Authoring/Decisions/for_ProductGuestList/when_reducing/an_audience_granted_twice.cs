// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.Decisions.for_ProductGuestList.when_reducing;

/// <summary>
/// An audience is held once however many times it was granted.
/// </summary>
public class an_audience_granted_twice : a_product_guest_list
{
    async Task Because() => await Happened(Guests.TenantGrant, Guests.TenantGrant);

    [Fact] void should_hold_it_once() => GuestList.Grants.Count().ShouldEqual(1);
}
#endif
