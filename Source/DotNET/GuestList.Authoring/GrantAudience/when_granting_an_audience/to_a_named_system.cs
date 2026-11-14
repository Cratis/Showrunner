// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.GrantAudience.when_granting_an_audience;

/// <summary>
/// Granting an audience to a named system names its client id and kind and no tenant or user.
/// </summary>
public class to_a_named_system : a_guest_list_command<GrantAudience>
{
    async Task Because() => _result = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.NamedSystem, TenantId.NotSet, UserId.NotSet, Guests.Plus, Guests.Crm, SystemKind.Mcp));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_that_it_was_granted_to_that_system() =>
        await _scenario.ShouldHaveAppendedEvent<GrantAudience, AudienceGrantDecided>(
            new EventSourceId(Guests.Product.Value),
            e => e.Level == AudienceLevel.NamedSystem && e.ClientId == Guests.Crm && e.Kind == SystemKind.Mcp && e.TenantId == TenantId.NotSet && e.UserId == UserId.NotSet && e.Audience == Guests.Plus);
}
#endif
