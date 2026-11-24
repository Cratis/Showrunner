// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.SuspendPart.when_suspending_a_part;

/// <summary>
/// A part suspended for the tenant can still be suspended for a user and for the system: each level is its own decision.
/// </summary>
public class to_a_user_and_the_system : a_guest_list_command<SuspendPart>
{
    CommandResult _user;
    CommandResult _system;

    void Establish() => Given(Guests.TenantSuspension);

    async Task Because()
    {
        _user = await _scenario.Execute(new SuspendPart(Guests.Product, SuspensionLevel.User, Guests.Acme, Guests.Alice, Guests.FleetReports));
        _system = await _scenario.Execute(new SuspendPart(Guests.Product, SuspensionLevel.System, TenantId.NotSet, UserId.NotSet, Guests.FleetReports));
    }

    [Fact] void should_suspend_for_the_user() => _user.ShouldBeSuccessful();

    [Fact] void should_suspend_for_the_system() => _system.ShouldBeSuccessful();

    [Fact]
    async Task should_append_the_user_suspension() =>
        await _scenario.ShouldHaveAppendedEvent<SuspendPart, PartSuspensionDecided>(
            new EventSourceId(Guests.Product.Value),
            e => e.Level == SuspensionLevel.User && e.TenantId == Guests.Acme && e.UserId == Guests.Alice);

    [Fact]
    async Task should_append_the_system_suspension() =>
        await _scenario.ShouldHaveAppendedEvent<SuspendPart, PartSuspensionDecided>(
            new EventSourceId(Guests.Product.Value),
            e => e.Level == SuspensionLevel.System && e.Part == Guests.FleetReports);
}
#endif
