// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;
using Cratis.Showrunner.Marquee.Receiving;

namespace Cratis.Showrunner.Marquee.Standing.for_StandingAnnouncement.when_reducing;

/// <summary>
/// Another audience changes who it reaches.
/// </summary>
public class an_announcement_given_another_audience : a_read_model<StandingAnnouncement>
{
    StandingAnnouncement _announcement;

    async Task Because()
    {
        await Happened(ProductAnnouncements.Received(ProductAnnouncements.DialogOnce(_announcementId)), new AnnouncementAudienceChangeReceived(Audience.OnPlan("team")));
        _announcement = _scenario.Instance;
    }

    [Fact] void should_be_for_the_plan() => (_announcement.Audience.Kind, _announcement.Audience.Plan).ShouldEqual((AudienceKind.OnPlan, new PlanId("team")));
}
#endif
