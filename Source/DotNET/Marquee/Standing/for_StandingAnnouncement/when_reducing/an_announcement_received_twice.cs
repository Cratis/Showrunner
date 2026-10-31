// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;
using Cratis.Showrunner.Marquee.Receiving;

namespace Cratis.Showrunner.Marquee.Standing.for_StandingAnnouncement.when_reducing;

/// <summary>
/// Receiving an announcement the product already holds changes nothing.
/// </summary>
public class an_announcement_received_twice : a_read_model<StandingAnnouncement>
{
    StandingAnnouncement _announcement;

    async Task Because()
    {
        await Happened(ProductAnnouncements.Received(ProductAnnouncements.DialogOnce(_announcementId)), ProductAnnouncements.Received(ProductAnnouncements.Banner(_announcementId)));
        _announcement = _scenario.Instance;
    }

    [Fact] void should_keep_the_first() => _announcement.Title.ShouldEqual(new AnnouncementTitle("Meet Reporting"));
}
#endif
