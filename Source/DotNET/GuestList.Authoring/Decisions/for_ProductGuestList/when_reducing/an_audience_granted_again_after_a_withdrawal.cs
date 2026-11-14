// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.Decisions.for_ProductGuestList.when_reducing;

/// <summary>
/// The newest decision wins: a grant after a withdrawal holds the audience again.
/// </summary>
public class an_audience_granted_again_after_a_withdrawal : a_product_guest_list
{
    async Task Because() => await Happened(Guests.TenantGrant, Guests.TenantWithdrawal, Guests.TenantGrant);

    [Fact] void should_hold_the_audience() => GuestList.Grants.ShouldContainOnly(new HeldAudience(AudienceLevel.Tenant, Guests.Acme, UserId.NotSet, Guests.Plus, SystemClientId.NotSet, SystemKind.None));
}
#endif
