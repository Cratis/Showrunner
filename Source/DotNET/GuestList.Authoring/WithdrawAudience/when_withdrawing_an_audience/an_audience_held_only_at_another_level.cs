// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.WithdrawAudience.when_withdrawing_an_audience;

/// <summary>
/// A withdrawal removes the grant of its own level only: withdrawing from a user an audience that only the tenant and the
/// system hold records nothing, and leaves both grants where they are.
/// </summary>
public class an_audience_held_only_at_another_level : a_guest_list_command<WithdrawAudience>
{
    void Establish() => Given(Guests.TenantGrant, Guests.SystemGrant);

    async Task Because() => _result = await _scenario.Execute(new WithdrawAudience(Guests.Product, AudienceLevel.User, Guests.Acme, Guests.Alice, Guests.Plus));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
