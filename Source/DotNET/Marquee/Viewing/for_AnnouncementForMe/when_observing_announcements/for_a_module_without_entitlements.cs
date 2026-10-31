// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;

namespace Cratis.Showrunner.Marquee.Viewing.for_AnnouncementForMe.when_observing_announcements;

/// <summary>
/// Without entitlements, a module audience never matches: a product that sells nothing gets no module announcements.
/// </summary>
public class for_a_module_without_entitlements : an_announcements_query
{
    protected override IEnumerable<StandingAnnouncement> Held => [ProductAnnouncements.Banner(AnnouncementId.New(), Audience.HoldersOfModule("reporting")), ProductAnnouncements.Banner(AnnouncementId.New(), Audience.OnPlan("team"))];

    protected override IAudienceEntitlements Entitlements => NoAudienceEntitlements.Instance;

    async Task Because() => await Ask();

    [Fact] void should_return_nothing() => Announcements.ShouldBeEmpty();
}
#endif
