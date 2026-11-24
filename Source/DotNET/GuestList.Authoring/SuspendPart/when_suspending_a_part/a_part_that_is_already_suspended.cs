// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.SuspendPart.when_suspending_a_part;

/// <summary>
/// Suspending a part that is already suspended at that level records nothing, so the decision is published once.
/// </summary>
public class a_part_that_is_already_suspended : a_guest_list_command<SuspendPart>
{
    void Establish() => Given(Guests.TenantSuspension);

    async Task Because() => _result = await _scenario.Execute(new SuspendPart(Guests.Product, SuspensionLevel.Tenant, Guests.Acme, UserId.NotSet, Guests.FleetReports));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
