// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;
using Cratis.Showrunner.Marquee.Receiving;

namespace Cratis.Showrunner.Marquee.Standing.for_StandingAnnouncement.when_reducing;

/// <summary>
/// A revision of an announcement the product never received does not make one.
/// </summary>
public class a_revision_of_an_announcement_that_was_never_received : a_read_model<StandingAnnouncement>
{
    StandingAnnouncement _announcement;

    async Task Because()
    {
        await Happened(new AnnouncementRevisionReceived("Ghost", "Ghost.", Importance.Warning, ProductAnnouncements.MaintenanceStarts, ProductAnnouncements.MaintenanceEnds));
        _announcement = _scenario.Instance;
    }

    [Fact] void should_not_hold_an_announcement() => _announcement.ShouldBeNull();
}
#endif
