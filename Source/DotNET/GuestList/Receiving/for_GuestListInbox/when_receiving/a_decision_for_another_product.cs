// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.given;
using Cratis.Showrunner.GuestList.Receiving.for_GuestListInbox.given;

namespace Cratis.Showrunner.GuestList.Receiving.for_GuestListInbox.when_receiving;

/// <summary>
/// An operator store serves several products: a decision made for another product is not this product's.
/// </summary>
public class a_decision_for_another_product : a_guest_list_inbox
{
    async Task Because()
    {
        await Delivered(new AudienceGranted("another", AudienceLevel.Tenant, Guests.Acme, UserId.NotSet, Guests.Plus, SystemClientId.NotSet, SystemKind.None));
        await Delivered(new AudienceGranted("another", AudienceLevel.System, TenantId.NotSet, UserId.NotSet, Guests.Plus, SystemClientId.NotSet, SystemKind.None));
        await ReadRecorded();
    }

    [Fact] void should_record_nothing_for_the_tenant() => _recordedForAcme.ShouldBeEmpty();
    [Fact] void should_record_nothing_for_the_system() => _recordedForTheProduct.ShouldBeEmpty();
}
#endif
