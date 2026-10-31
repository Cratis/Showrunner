// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.RetargetAnnouncement.when_retargeting_an_announcement;

/// <summary>
/// The same tenants in another order are the same audience: nothing is recorded.
/// </summary>
public class the_same_tenants_in_another_order : an_announcement_command<RetargetAnnouncement>
{
    void Establish() => Given(AnnouncementEvents.Maintenance, new AnnouncementRetargeted(Audience.Tenants("fjord", "harbor")));

    async Task Because() => _result = await _scenario.Execute(new RetargetAnnouncement(_announcementId, Audience.Tenants("harbor", "fjord")));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
