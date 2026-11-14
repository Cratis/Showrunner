// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.WithdrawAudience.when_withdrawing_an_audience;

/// <summary>
/// Withdrawing an audience from every system removes the product-wide grant only: when just one named system holds it, the
/// withdrawal records nothing and leaves that grant where it is.
/// </summary>
public class an_audience_held_only_by_a_named_system : a_guest_list_command<WithdrawAudience>
{
    void Establish() => Given(new AudienceGrantDecided(AudienceLevel.NamedSystem, TenantId.NotSet, UserId.NotSet, Guests.Plus, Guests.Crm, SystemKind.Mcp));

    async Task Because() => _result = await _scenario.Execute(new WithdrawAudience(Guests.Product, AudienceLevel.System, TenantId.NotSet, UserId.NotSet, Guests.Plus));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
