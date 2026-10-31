// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;

namespace Cratis.Showrunner.Marquee.Viewing.for_AnnouncementForMe.when_observing_announcements;

/// <summary>
/// Somebody else having acknowledged a dialog does not hide it from this person.
/// </summary>
public class a_dialog_somebody_else_acknowledged : an_announcements_query
{
    readonly AnnouncementId _announcementId = AnnouncementId.New();

    protected override IEnumerable<StandingAnnouncement> Held => [ProductAnnouncements.DialogOnce(_announcementId)];

    void Establish() => Given(_announcementId, new AnnouncementAcknowledged(_announcementId, Viewers.SomebodyElse));

    async Task Because() => await Ask();

    [Fact] void should_still_return_it() => Announcements.Select(announcement => announcement.Id).ShouldEqual([_announcementId]);
}
#endif
