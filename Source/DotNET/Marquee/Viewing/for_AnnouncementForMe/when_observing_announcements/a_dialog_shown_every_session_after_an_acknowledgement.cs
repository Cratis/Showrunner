// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;

namespace Cratis.Showrunner.Marquee.Viewing.for_AnnouncementForMe.when_observing_announcements;

/// <summary>
/// A dialog shown every session comes back whatever was recorded: only a dialog shown once is hidden by an acknowledgement.
/// </summary>
public class a_dialog_shown_every_session_after_an_acknowledgement : an_announcements_query
{
    readonly AnnouncementId _announcementId = AnnouncementId.New();

    protected override IEnumerable<StandingAnnouncement> Held => [ProductAnnouncements.DialogEverySession(_announcementId)];

    void Establish() => Given(_announcementId, new AnnouncementAcknowledged(_announcementId, Viewers.Me));

    async Task Because() => await Ask();

    [Fact] void should_still_return_it() => Announcements.Select(announcement => announcement.Id).ShouldEqual([_announcementId]);
}
#endif
