// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.AmendAnnouncement.when_amending_an_announcement;

/// <summary>
/// Amending appends what changed; display and audience are not part of it.
/// </summary>
public class an_announcement_with_a_new_title_and_window : an_announcement_command<AmendAnnouncement>
{
    void Establish() => Given(AnnouncementEvents.Maintenance);

    async Task Because() => _result = await _scenario.Execute(new AmendAnnouncement(_announcementId, "Maintenance moved", "Ledger is read-only for three hours.", Importance.Critical, AnnouncementEvents.MaintenanceStarts, AnnouncementEvents.MaintenanceEnds.AddHours(1)));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_that_it_was_amended() =>
        await _scenario.ShouldHaveAppendedEvent<AmendAnnouncement, AnnouncementAmended>(
            _announcementId,
            e => e.Title == "Maintenance moved" &&
                 e.Body == "Ledger is read-only for three hours." &&
                 e.Importance == Importance.Critical &&
                 e.ActiveFrom.Value == AnnouncementEvents.MaintenanceStarts &&
                 e.ActiveUntil.Value == AnnouncementEvents.MaintenanceEnds.AddHours(1));
}
#endif
