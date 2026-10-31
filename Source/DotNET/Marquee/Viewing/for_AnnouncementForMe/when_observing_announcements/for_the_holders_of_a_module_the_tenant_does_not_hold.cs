// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;

namespace Cratis.Showrunner.Marquee.Viewing.for_AnnouncementForMe.when_observing_announcements;

/// <summary>
/// A tenant that does not hold the module is not reached.
/// </summary>
public class for_the_holders_of_a_module_the_tenant_does_not_hold : an_announcements_query
{
    protected override IEnumerable<StandingAnnouncement> Held => [ProductAnnouncements.Banner(AnnouncementId.New(), Audience.HoldersOfModule("reporting"))];

    protected override IAudienceEntitlements Entitlements => new FixedEntitlements([ProductAnnouncements.AnotherTenant], []);

    async Task Because() => await Ask();

    [Fact] void should_return_nothing() => Announcements.ShouldBeEmpty();
}
#endif
