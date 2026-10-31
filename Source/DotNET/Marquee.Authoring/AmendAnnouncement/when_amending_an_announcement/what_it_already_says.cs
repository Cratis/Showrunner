// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.AmendAnnouncement.when_amending_an_announcement;

/// <summary>
/// Amending to what the announcement already says records nothing.
/// </summary>
public class what_it_already_says : an_announcement_command<AmendAnnouncement>
{
    void Establish() => Given(AnnouncementEvents.Maintenance);

    async Task Because() => _result = await _scenario.Execute(new AmendAnnouncement(_announcementId, "Scheduled maintenance", "Ledger is read-only for two hours.", Importance.Warning, AnnouncementEvents.MaintenanceStarts, AnnouncementEvents.MaintenanceEnds));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
