// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;
using Cratis.Showrunner.GuestList.Authoring.Publishing.for_GuestListOutbox.given;

namespace Cratis.Showrunner.GuestList.Authoring.Publishing.for_GuestListOutbox.when_publishing;

/// <summary>
/// A grant and its withdrawal are published in the order they were decided, as the two contract events.
/// </summary>
public class a_withdrawal : a_guest_list_outbox
{
    async Task Because()
    {
        await Happened(Guests.TenantGrant);
        await Happened(Guests.TenantWithdrawal);
        await ReadPublished();
    }

    [Fact] void should_publish_both() => _published.Count.ShouldEqual(2);
    [Fact] void should_publish_the_grant_first() => _published[0].Content.ShouldEqual(new AudienceGranted(Guests.Product, AudienceLevel.Tenant, Guests.Acme, UserId.NotSet, Guests.Plus));
    [Fact] void should_publish_the_withdrawal_after_it() => _published[1].Content.ShouldEqual(new AudienceWithdrawn(Guests.Product, AudienceLevel.Tenant, Guests.Acme, UserId.NotSet, Guests.Plus));
    [Fact] void should_keep_the_time_the_decision_was_taken() => (_published[1].Context.Occurred >= _published[0].Context.Occurred).ShouldBeTrue();
}
#endif
