// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.SuspendPart.when_suspending_a_part;

/// <summary>
/// A part that was suspended and resumed can be suspended again.
/// </summary>
public class a_part_that_was_resumed : a_guest_list_command<SuspendPart>
{
    void Establish() => Given(Guests.TenantSuspension, new PartResumeDecided(SuspensionLevel.Tenant, Guests.Acme, UserId.NotSet, Guests.FleetReports));

    async Task Because() => _result = await _scenario.Execute(new SuspendPart(Guests.Product, SuspensionLevel.Tenant, Guests.Acme, UserId.NotSet, Guests.FleetReports));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_that_it_was_suspended() =>
        await _scenario.ShouldHaveAppendedEvent<SuspendPart, PartSuspensionDecided>(
            new EventSourceId(Guests.Product.Value),
            e => e.Level == SuspensionLevel.Tenant && e.TenantId == Guests.Acme && e.Part == Guests.FleetReports);
}
#endif
