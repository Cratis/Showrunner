// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.Announcements.for_Announcement.when_reducing;

/// <summary>
/// A withdrawn announcement stays, withdrawn, so the authoring screen can show it.
/// </summary>
public class a_withdrawn_announcement : an_announcement_read_model
{
    Announcement _announcement;

    async Task Because()
    {
        await Happened(AnnouncementEvents.Maintenance, new AnnouncementWithdrawn());
        _announcement = _scenario.Instance;
    }

    [Fact] void should_be_withdrawn() => _announcement.Withdrawn.ShouldBeTrue();

    [Fact] void should_keep_what_it_said() => _announcement.Title.ShouldEqual(new AnnouncementTitle("Scheduled maintenance"));
}
#endif
