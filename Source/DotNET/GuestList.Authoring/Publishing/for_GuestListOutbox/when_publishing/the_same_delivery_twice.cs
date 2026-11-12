// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;
using Cratis.Showrunner.GuestList.Authoring.Publishing.for_GuestListOutbox.given;

namespace Cratis.Showrunner.GuestList.Authoring.Publishing.for_GuestListOutbox.when_publishing;

/// <summary>
/// A redelivery appends nothing a second time.
/// </summary>
public class the_same_delivery_twice : a_guest_list_outbox
{
    async Task Because()
    {
        await Recorded(Guests.TenantGrant, Guests.TenantWithdrawal);
        await DeliverEverything();
        await DeliverEverything();
        await ReadPublished();
    }

    [Fact] void should_publish_each_decision_once() => _published.Count.ShouldEqual(2);
}
#endif
