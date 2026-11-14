// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.AmendAnnouncement.when_amending_an_announcement;

/// <summary>
/// A client that leaves the message out altogether still amends the announcement, and it is kept with an empty message.
/// </summary>
public class with_the_message_left_out : an_announcement_command<AmendAnnouncement>
{
    void Establish() => Given(AnnouncementEvents.Maintenance);

    async Task Because() => _result = await _scenario.Execute(new AmendAnnouncement(
        _announcementId,
        "Maintenance moved",
        null,
        Importance.Warning,
        AnnouncementEvents.MaintenanceStarts,
        AnnouncementEvents.MaintenanceEnds));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_it_with_an_empty_message() =>
        await _scenario.ShouldHaveAppendedEvent<AmendAnnouncement, AnnouncementAmended>(_announcementId, e => e.Body == AnnouncementBody.NotSet);
}
#endif
