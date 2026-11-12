// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.GrantAudience.when_granting_an_audience;

/// <summary>
/// The levels are separate: an audience the tenant holds is still granted to a user of it.
/// </summary>
public class an_audience_held_at_another_level : a_guest_list_command<GrantAudience>
{
    void Establish() => Given(Guests.TenantGrant, Guests.SystemGrant);

    async Task Because() => _result = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.User, Guests.Acme, Guests.Alice, Guests.Plus));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_that_it_was_granted() =>
        await _scenario.ShouldHaveAppendedEvent<GrantAudience, AudienceGrantDecided>(new EventSourceId(Guests.Product.Value), e => e.Level == AudienceLevel.User);
}
#endif
