// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.Announcements.for_Announcement.when_reducing;

/// <summary>
/// Retargeting changes who it is for.
/// </summary>
public class a_retargeted_announcement : an_announcement_read_model
{
    Announcement _announcement;

    async Task Because()
    {
        await Happened(AnnouncementEvents.Maintenance, new AnnouncementRetargeted(Audience.OnPlan("team")));
        _announcement = _scenario.Instance;
    }

    [Fact] void should_be_for_the_plan() => _announcement.Audience.Plan.ShouldEqual(new PlanId("team"));

    [Fact] void should_be_for_the_kind_of_audience() => _announcement.Audience.Kind.ShouldEqual(AudienceKind.OnPlan);
}
#endif
