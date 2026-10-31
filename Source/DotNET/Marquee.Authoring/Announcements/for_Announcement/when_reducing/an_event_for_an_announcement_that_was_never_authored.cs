// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.Announcements.for_Announcement.when_reducing;

/// <summary>
/// A fact about an announcement that was never authored does not make one.
/// </summary>
public class an_event_for_an_announcement_that_was_never_authored : an_announcement_read_model
{
    Announcement _announcement;

    async Task Because()
    {
        await Happened(new AnnouncementWithdrawn());
        _announcement = _scenario.Instance;
    }

    [Fact] void should_not_have_an_announcement() => _announcement.ShouldBeNull();
}
#endif
