// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;
using Cratis.Showrunner.GuestList.Authoring.Publishing.for_GuestListOutbox.given;

namespace Cratis.Showrunner.GuestList.Authoring.Publishing.for_GuestListOutbox.when_publishing;

/// <summary>
/// A decision about one named system is published as the contract event with the client id and the kind of the system.
/// </summary>
public class a_grant_and_a_withdrawal_for_a_named_system : a_guest_list_outbox
{
    async Task Because()
    {
        await Happened(Guests.CrmGrant);
        await Happened(Guests.CrmWithdrawal);
        await ReadPublished();
    }

    [Fact] void should_publish_both() => _published.Count.ShouldEqual(2);
    [Fact] void should_publish_the_grant() => _published[0].Content.ShouldEqual(new AudienceGranted(Guests.Product, AudienceLevel.NamedSystem, TenantId.NotSet, UserId.NotSet, Guests.Plus, Guests.Crm, SystemKind.Mcp));
    [Fact] void should_publish_the_withdrawal() => _published[1].Content.ShouldEqual(new AudienceWithdrawn(Guests.Product, AudienceLevel.NamedSystem, TenantId.NotSet, UserId.NotSet, Guests.Plus, Guests.Crm, SystemKind.Mcp));
}
#endif
