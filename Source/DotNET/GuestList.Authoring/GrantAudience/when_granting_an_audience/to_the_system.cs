// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.GrantAudience.when_granting_an_audience;

/// <summary>
/// Granting an audience to the system names no tenant and no user.
/// </summary>
public class to_the_system : a_guest_list_command<GrantAudience>
{
    async Task Because() => _result = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.System, TenantId.NotSet, UserId.NotSet, Guests.Plus));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_that_it_was_granted() =>
        await _scenario.ShouldHaveAppendedEvent<GrantAudience, AudienceGrantDecided>(
            new EventSourceId(Guests.Product.Value),
            e => e.Level == AudienceLevel.System && e.TenantId == TenantId.NotSet && e.UserId == UserId.NotSet && e.Audience == Guests.Plus);
}
#endif
