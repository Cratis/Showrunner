// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;

namespace Cratis.Showrunner.Marquee.Viewing.for_AnnouncementForMe.when_observing_announcements;

/// <summary>
/// Nobody who is not signed in is shown anything.
/// </summary>
public class a_person_who_is_not_signed_in : an_announcements_query
{
    protected override System.Security.Claims.ClaimsPrincipal Principal => Viewers.Anonymous;

    protected override IEnumerable<StandingAnnouncement> Held => [ProductAnnouncements.Banner(AnnouncementId.New())];

    async Task Because() => await Ask();

    [Fact] void should_not_be_authorized() => _result.IsAuthorized.ShouldBeFalse();
}
#endif
