// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.GrantAudience.when_granting_an_audience;

/// <summary>
/// Granting an audience that is already held at that level is harmless and records nothing, so the decision is published
/// once.
/// </summary>
public class an_audience_that_is_already_held : a_guest_list_command<GrantAudience>
{
    void Establish() => Given(Guests.TenantGrant);

    async Task Because() => _result = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.Tenant, Guests.Acme, UserId.NotSet, Guests.Plus));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
