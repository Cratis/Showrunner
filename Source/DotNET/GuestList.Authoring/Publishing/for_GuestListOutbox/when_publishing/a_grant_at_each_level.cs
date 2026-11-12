// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Authoring.given;
using Cratis.Showrunner.GuestList.Authoring.Publishing.for_GuestListOutbox.given;

namespace Cratis.Showrunner.GuestList.Authoring.Publishing.for_GuestListOutbox.when_publishing;

/// <summary>
/// A grant at each level is published as the contract event, under the product, with what the level names.
/// </summary>
public class a_grant_at_each_level : a_guest_list_outbox
{
    async Task Because()
    {
        await Happened(Guests.TenantGrant);
        await Happened(Guests.UserGrant);
        await Happened(Guests.SystemGrant);
        await ReadPublished();
    }

    [Fact] void should_publish_each_grant() => _published.Count.ShouldEqual(3);
    [Fact] void should_publish_them_under_the_product() => _published.All(_ => _.Context.EventSourceId == new EventSourceId(Guests.Product.Value)).ShouldBeTrue();
    [Fact] void should_publish_the_tenants_grant() => _published[0].Content.ShouldEqual(new AudienceGranted(Guests.Product, AudienceLevel.Tenant, Guests.Acme, UserId.NotSet, Guests.Plus));
    [Fact] void should_publish_the_users_grant() => _published[1].Content.ShouldEqual(new AudienceGranted(Guests.Product, AudienceLevel.User, Guests.Acme, Guests.Alice, Guests.Plus));
    [Fact] void should_publish_the_systems_grant() => _published[2].Content.ShouldEqual(new AudienceGranted(Guests.Product, AudienceLevel.System, TenantId.NotSet, UserId.NotSet, Guests.Plus));
}
#endif
