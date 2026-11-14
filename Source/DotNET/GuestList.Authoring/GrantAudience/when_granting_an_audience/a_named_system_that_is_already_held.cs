// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.GrantAudience.when_granting_an_audience;

/// <summary>
/// A system is who its client id says it is, so granting an audience the system already holds records nothing, whatever kind
/// the command says.
/// </summary>
public class a_named_system_that_is_already_held : a_guest_list_command<GrantAudience>
{
    void Establish() => Given(new AudienceGrantDecided(AudienceLevel.NamedSystem, TenantId.NotSet, UserId.NotSet, Guests.Plus, Guests.Crm, SystemKind.Mcp));

    async Task Because() => _result = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.NamedSystem, TenantId.NotSet, UserId.NotSet, Guests.Plus, Guests.Crm, SystemKind.Other));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
