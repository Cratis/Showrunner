// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.Announcements.for_Announcement.when_reducing;

/// <summary>
/// An announcement that is authored is shown with everything it was authored with.
/// </summary>
public class an_authored_announcement : an_announcement_read_model
{
    Announcement _announcement;

    async Task Because()
    {
        await Happened(AnnouncementEvents.Maintenance);
        _announcement = _scenario.Instance;
    }

    [Fact] void should_have_the_id_of_the_event_source() => _announcement.Id.ShouldEqual(_announcementId);

    [Fact] void should_be_for_the_product() => _announcement.Product.ShouldEqual(AnnouncementEvents.Product);

    [Fact] void should_have_the_title() => _announcement.Title.ShouldEqual(new AnnouncementTitle("Scheduled maintenance"));

    [Fact] void should_have_the_window() => (_announcement.ActiveFrom.Value, _announcement.ActiveUntil.Value).ShouldEqual((AnnouncementEvents.MaintenanceStarts, AnnouncementEvents.MaintenanceEnds));

    [Fact] void should_not_be_withdrawn() => _announcement.Withdrawn.ShouldBeFalse();
}
#endif
