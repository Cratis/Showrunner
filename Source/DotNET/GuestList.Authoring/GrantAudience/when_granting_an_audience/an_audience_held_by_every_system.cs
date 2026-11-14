// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.GrantAudience.when_granting_an_audience;

/// <summary>
/// The product-wide system grant and the grant to one named system are separate holders: granting an audience to a named
/// system is still recorded when every system already holds it.
/// </summary>
public class an_audience_held_by_every_system : a_guest_list_command<GrantAudience>
{
    void Establish() => Given(Guests.SystemGrant);

    async Task Because() => _result = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.NamedSystem, TenantId.NotSet, UserId.NotSet, Guests.Plus, Guests.Crm, SystemKind.Api));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_that_it_was_granted() =>
        await _scenario.ShouldHaveAppendedEvent<GrantAudience, AudienceGrantDecided>(new EventSourceId(Guests.Product.Value), e => e.Level == AudienceLevel.NamedSystem);
}
#endif
