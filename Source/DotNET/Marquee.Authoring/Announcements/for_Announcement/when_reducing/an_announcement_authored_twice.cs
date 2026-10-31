// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.Announcements.for_Announcement.when_reducing;

/// <summary>
/// Authoring an announcement that already exists changes nothing.
/// </summary>
public class an_announcement_authored_twice : an_announcement_read_model
{
    Announcement _announcement;

    async Task Because()
    {
        await Happened(AnnouncementEvents.Maintenance, AnnouncementEvents.NewModule);
        _announcement = _scenario.Instance;
    }

    [Fact] void should_keep_the_first_title() => _announcement.Title.ShouldEqual(new AnnouncementTitle("Scheduled maintenance"));
}
#endif
