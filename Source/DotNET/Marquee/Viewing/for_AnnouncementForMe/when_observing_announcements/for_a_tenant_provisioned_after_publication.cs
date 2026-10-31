// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;

namespace Cratis.Showrunner.Marquee.Viewing.for_AnnouncementForMe.when_observing_announcements;

/// <summary>
/// A tenant created after an announcement was published still sees it while it is active: the announcement is the
/// product's, held once, and nothing was copied to the tenants that existed when it arrived.
/// </summary>
public class for_a_tenant_provisioned_after_publication : an_announcements_query
{
    readonly AnnouncementId _announcementId = AnnouncementId.New();

    protected override TenantId Tenant => new("newcomer");

    protected override IEnumerable<StandingAnnouncement> Held => [ProductAnnouncements.Banner(_announcementId)];

    async Task Because() => await Ask();

    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();

    [Fact] void should_see_the_announcement() => Announcements.Select(announcement => announcement.Id).ShouldEqual([_announcementId]);
}
#endif
