// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.Decisions.for_ProductGuestList.when_reducing;

/// <summary>
/// A withdrawal removes the grant of its own level only: the same audience granted to the tenant, a user and the system
/// stays with the other two when the tenant's is withdrawn.
/// </summary>
public class grants_and_a_withdrawal : a_product_guest_list
{
    async Task Because() => await Happened(Guests.TenantGrant, Guests.UserGrant, Guests.SystemGrant, Guests.TenantWithdrawal);

    [Fact] void should_belong_to_the_product() => GuestList.Product.ShouldEqual(Guests.Product);

    [Fact]
    void should_hold_what_was_not_withdrawn() =>
        GuestList.Grants.ShouldContainOnly(
            new HeldAudience(AudienceLevel.User, Guests.Acme, Guests.Alice, Guests.Plus),
            new HeldAudience(AudienceLevel.System, TenantId.NotSet, UserId.NotSet, Guests.Plus));
}
#endif
