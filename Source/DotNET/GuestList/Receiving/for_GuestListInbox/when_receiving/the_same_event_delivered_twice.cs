// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.given;
using Cratis.Showrunner.GuestList.Receiving.for_GuestListInbox.given;

namespace Cratis.Showrunner.GuestList.Receiving.for_GuestListInbox.when_receiving;

/// <summary>
/// A repeated delivery changes nothing: the second copy of the same event yields no second fact.
/// </summary>
public class the_same_event_delivered_twice : a_guest_list_inbox
{
    async Task Because()
    {
        var context = NextContext();
        await Delivered(Guests.GrantToTenant(Guests.Plus), context);
        await Delivered(Guests.GrantToTenant(Guests.Plus), context);
        await ReadRecorded();
    }

    [Fact] void should_record_one_fact() => _recordedForAcme.Count.ShouldEqual(1);
}
#endif
