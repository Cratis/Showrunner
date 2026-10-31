// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;

namespace Cratis.Showrunner.Marquee.Viewing.for_AnnouncementForMe.when_observing_announcements;

/// <summary>
/// An announcement for named tenants reaches those tenants only.
/// </summary>
public class for_a_tenant_that_is_named : an_announcements_query
{
    readonly AnnouncementId _mine = AnnouncementId.New();
    readonly AnnouncementId _theirs = AnnouncementId.New();

    protected override IEnumerable<StandingAnnouncement> Held =>
    [
        ProductAnnouncements.Banner(_mine, Audience.Tenants(ProductAnnouncements.Tenant)),
        ProductAnnouncements.Banner(_theirs, Audience.Tenants(ProductAnnouncements.AnotherTenant))
    ];

    async Task Because() => await Ask();

    [Fact] void should_return_the_one_for_the_tenant() => Announcements.Select(announcement => announcement.Id).ShouldEqual([_mine]);
}
#endif
