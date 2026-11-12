// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.given;
using Cratis.Showrunner.GuestList.Receiving.for_GuestListInbox.given;

namespace Cratis.Showrunner.GuestList.Receiving.for_GuestListInbox.when_receiving;

/// <summary>
/// A decision never creates a tenant: one about a tenant the product has no namespace for is not recorded.
/// </summary>
public class a_decision_for_a_tenant_the_product_does_not_have : a_guest_list_inbox
{
    async Task Because()
    {
        await Delivered(Guests.GrantToTenant(Guests.Plus, Guests.Globex));
        await Delivered(Guests.GrantToUser(Guests.Plus) with { TenantId = Guests.Globex });
        await ReadRecorded();
    }

    [Fact] void should_record_nothing_for_the_tenant() => _recordedForAcme.ShouldBeEmpty();
    [Fact] void should_record_nothing_in_the_products_own_namespace() => _recordedForTheProduct.ShouldBeEmpty();
}
#endif
