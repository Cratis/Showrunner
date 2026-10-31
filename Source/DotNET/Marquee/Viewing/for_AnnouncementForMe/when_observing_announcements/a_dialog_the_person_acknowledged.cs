// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;

namespace Cratis.Showrunner.Marquee.Viewing.for_AnnouncementForMe.when_observing_announcements;

/// <summary>
/// A dialog shown once that the person acknowledged is left out.
/// </summary>
public class a_dialog_the_person_acknowledged : an_announcements_query
{
    readonly AnnouncementId _acknowledged = AnnouncementId.New();
    readonly AnnouncementId _unseen = AnnouncementId.New();

    protected override IEnumerable<StandingAnnouncement> Held => [ProductAnnouncements.DialogOnce(_acknowledged), ProductAnnouncements.DialogOnce(_unseen)];

    void Establish() => Given(_acknowledged, new AnnouncementAcknowledged(_acknowledged, Viewers.Me));

    async Task Because() => await Ask();

    [Fact] void should_leave_it_out() => Announcements.Select(announcement => announcement.Id).ShouldEqual([_unseen]);
}
#endif
