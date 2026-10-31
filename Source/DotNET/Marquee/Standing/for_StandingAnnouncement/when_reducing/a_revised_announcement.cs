// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;
using Cratis.Showrunner.Marquee.Receiving;

namespace Cratis.Showrunner.Marquee.Standing.for_StandingAnnouncement.when_reducing;

/// <summary>
/// A revision changes what it says and when it is shown, not how it is displayed or who it is for.
/// </summary>
public class a_revised_announcement : a_read_model<StandingAnnouncement>
{
    StandingAnnouncement _announcement;

    async Task Because()
    {
        await Happened(ProductAnnouncements.Received(ProductAnnouncements.DialogOnce(_announcementId)), new AnnouncementRevisionReceived("Reporting is here", "New.", Importance.Warning, ProductAnnouncements.MaintenanceStarts, ProductAnnouncements.MaintenanceEnds));
        _announcement = _scenario.Instance;
    }

    [Fact] void should_have_the_new_title() => _announcement.Title.ShouldEqual(new AnnouncementTitle("Reporting is here"));

    [Fact] void should_have_the_new_window() => (_announcement.ActiveFrom.Value, _announcement.ActiveUntil.Value).ShouldEqual((ProductAnnouncements.MaintenanceStarts, ProductAnnouncements.MaintenanceEnds));

    [Fact] void should_keep_how_it_is_displayed() => _announcement.Display.ShouldEqual(Display.DialogOnce);
}
#endif
