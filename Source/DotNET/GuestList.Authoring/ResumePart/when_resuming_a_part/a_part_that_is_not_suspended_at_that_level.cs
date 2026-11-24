// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.ResumePart.when_resuming_a_part;

/// <summary>
/// Resuming a part that is not suspended at that level records nothing: a resume ends the suspension of its own level only,
/// so a part suspended for the tenant is not resumed by resuming it for the system, and a part never suspended is not
/// resumed at all.
/// </summary>
public class a_part_that_is_not_suspended_at_that_level : a_guest_list_command<ResumePart>
{
    CommandResult _system;
    CommandResult _never;

    void Establish() => Given(Guests.TenantSuspension);

    async Task Because()
    {
        _system = await _scenario.Execute(new ResumePart(Guests.Product, SuspensionLevel.System, TenantId.NotSet, UserId.NotSet, Guests.FleetReports));
        _never = await _scenario.Execute(new ResumePart(Guests.Product, SuspensionLevel.Tenant, Guests.Acme, UserId.NotSet, "Fleet/Other"));
    }

    [Fact] void should_succeed_for_the_system() => _system.ShouldBeSuccessful();

    [Fact] void should_succeed_for_a_part_never_suspended() => _never.ShouldBeSuccessful();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
