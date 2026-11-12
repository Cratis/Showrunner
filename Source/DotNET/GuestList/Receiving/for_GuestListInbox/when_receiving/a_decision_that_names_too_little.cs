// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.given;
using Cratis.Showrunner.GuestList.Receiving.for_GuestListInbox.given;

namespace Cratis.Showrunner.GuestList.Receiving.for_GuestListInbox.when_receiving;

/// <summary>
/// A decision that names no audience, or no tenant or user its level needs, is not recorded.
/// </summary>
public class a_decision_that_names_too_little : a_guest_list_inbox
{
    async Task Because()
    {
        await Delivered(Guests.GrantToTenant(string.Empty));
        await Delivered(Guests.GrantToTenant(Guests.Plus, TenantId.NotSet));
        await Delivered(Guests.GrantToUser(Guests.Plus, UserId.NotSet));
        await Delivered(Guests.GrantToSystem(string.Empty));
        await Delivered(Guests.GrantToTenant(Guests.Plus) with { Level = (AudienceLevel)42 });
        await ReadRecorded();
    }

    [Fact] void should_record_nothing_for_the_tenant() => _recordedForAcme.ShouldBeEmpty();
    [Fact] void should_record_nothing_in_the_products_own_namespace() => _recordedForTheProduct.ShouldBeEmpty();
}
#endif
