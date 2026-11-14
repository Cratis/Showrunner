// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.given;
using Cratis.Showrunner.GuestList.Receiving.for_GuestListInbox.given;

namespace Cratis.Showrunner.GuestList.Receiving.for_GuestListInbox.when_receiving;

/// <summary>
/// A decision for a named system that names no client id is not recorded: it must never turn into a grant to every system.
/// </summary>
public class a_named_system_that_names_no_client : a_guest_list_inbox
{
    async Task Because()
    {
        await Delivered(Guests.GrantToNamedSystem(Guests.Plus, SystemClientId.NotSet));
        await Delivered(Guests.GrantToNamedSystem(Guests.Plus, "  "));
        await Delivered(Guests.WithdrawFromNamedSystem(Guests.Plus, SystemClientId.NotSet));
        await ReadRecorded();
    }

    [Fact] void should_record_nothing_in_the_products_own_namespace() => _recordedForTheProduct.ShouldBeEmpty();
    [Fact] void should_record_nothing_for_the_tenant() => _recordedForAcme.ShouldBeEmpty();
}
#endif
