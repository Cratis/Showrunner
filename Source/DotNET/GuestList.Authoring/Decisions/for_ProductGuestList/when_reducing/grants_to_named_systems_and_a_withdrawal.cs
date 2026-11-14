// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.Decisions.for_ProductGuestList.when_reducing;

/// <summary>
/// Each named system holds its own grant, apart from every system's: withdrawing an audience from one system leaves the
/// other system's and the product-wide grants in place.
/// </summary>
public class grants_to_named_systems_and_a_withdrawal : a_product_guest_list
{
    async Task Because() => await Happened(Guests.SystemGrant, Guests.CrmGrant, Guests.CrmGrant, Guests.ReportsGrant, Guests.CrmWithdrawal);

    [Fact]
    void should_hold_what_was_not_withdrawn() =>
        GuestList.Grants.ShouldContainOnly(
            new HeldAudience(AudienceLevel.System, TenantId.NotSet, UserId.NotSet, Guests.Plus, SystemClientId.NotSet, SystemKind.None),
            new HeldAudience(AudienceLevel.NamedSystem, TenantId.NotSet, UserId.NotSet, Guests.Plus, Guests.Reports, SystemKind.Api));
}
#endif
