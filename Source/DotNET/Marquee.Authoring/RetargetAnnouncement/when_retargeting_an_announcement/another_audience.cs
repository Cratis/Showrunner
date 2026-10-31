// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.RetargetAnnouncement.when_retargeting_an_announcement;

/// <summary>
/// Retargeting appends the new audience.
/// </summary>
public class another_audience : an_announcement_command<RetargetAnnouncement>
{
    void Establish() => Given(AnnouncementEvents.Maintenance);

    async Task Because() => _result = await _scenario.Execute(new RetargetAnnouncement(_announcementId, Audience.HoldersOfModule("reporting")));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_the_new_audience() =>
        await _scenario.ShouldHaveAppendedEvent<RetargetAnnouncement, AnnouncementRetargeted>(_announcementId);

    [Fact]
    void should_append_one_event() => _scenario.AppendedEvents.Count.ShouldEqual(1);
}
#endif
