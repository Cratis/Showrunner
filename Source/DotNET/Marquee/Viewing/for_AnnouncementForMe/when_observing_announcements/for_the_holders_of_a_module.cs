// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;

namespace Cratis.Showrunner.Marquee.Viewing.for_AnnouncementForMe.when_observing_announcements;

/// <summary>
/// An announcement for the holders of a module reaches the tenants that hold it, as the host's entitlements answer.
/// </summary>
public class for_the_holders_of_a_module : an_announcements_query
{
    readonly AnnouncementId _announcementId = AnnouncementId.New();

    protected override IEnumerable<StandingAnnouncement> Held => [ProductAnnouncements.Banner(_announcementId, Audience.HoldersOfModule("reporting"))];

    protected override IAudienceEntitlements Entitlements => new FixedEntitlements([ProductAnnouncements.Tenant], []);

    async Task Because() => await Ask();

    [Fact] void should_return_it() => Announcements.Select(announcement => announcement.Id).ShouldEqual([_announcementId]);
}
#endif
