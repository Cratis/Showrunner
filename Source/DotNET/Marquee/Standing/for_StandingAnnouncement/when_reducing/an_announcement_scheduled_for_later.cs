// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;
using Cratis.Showrunner.Marquee.Receiving;

namespace Cratis.Showrunner.Marquee.Standing.for_StandingAnnouncement.when_reducing;

/// <summary>
/// Nothing is filtered by date on the server: an announcement that starts in the future is held from the moment it is received.
/// </summary>
public class an_announcement_scheduled_for_later : a_read_model<StandingAnnouncement>
{
    StandingAnnouncement _announcement;

    async Task Because()
    {
        await Happened(ProductAnnouncements.Received(ProductAnnouncements.Banner(_announcementId)));
        _announcement = _scenario.Instance;
    }

    [Fact] void should_be_held() => _announcement.ShouldNotBeNull();

    [Fact] void should_keep_when_it_starts() => _announcement.ActiveFrom.Value.ShouldEqual(ProductAnnouncements.MaintenanceStarts);
}
#endif
