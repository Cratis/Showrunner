// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.Announcements.for_Announcement.when_reducing;

/// <summary>
/// Amending changes what it says and when it is shown, not how it is displayed or who it is for.
/// </summary>
public class an_amended_announcement : an_announcement_read_model
{
    Announcement _announcement;

    async Task Because()
    {
        await Happened(AnnouncementEvents.Maintenance, new AnnouncementAmended("Maintenance moved", "Later.", Importance.Critical, AnnouncementEvents.MaintenanceEnds, ActiveUntil.Indefinitely));
        _announcement = _scenario.Instance;
    }

    [Fact] void should_have_the_new_title() => _announcement.Title.ShouldEqual(new AnnouncementTitle("Maintenance moved"));

    [Fact] void should_have_the_new_importance() => _announcement.Importance.ShouldEqual(Importance.Critical);

    [Fact] void should_have_the_new_window() => (_announcement.ActiveFrom.Value, _announcement.ActiveUntil).ShouldEqual((AnnouncementEvents.MaintenanceEnds, ActiveUntil.Indefinitely));

    [Fact] void should_keep_how_it_is_displayed() => _announcement.Display.ShouldEqual(Display.Banner);

    [Fact] void should_keep_who_it_is_for() => _announcement.Audience.Kind.ShouldEqual(AudienceKind.Everyone);
}
#endif
