// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.WithdrawAnnouncement.when_withdrawing_an_announcement;

/// <summary>
/// An announcement scheduled for later can be withdrawn before it ever showed.
/// </summary>
public class a_scheduled_announcement : an_announcement_command<WithdrawAnnouncement>
{
    void Establish() => Given(AnnouncementEvents.Maintenance);

    async Task Because() => _result = await _scenario.Execute(new WithdrawAnnouncement(_announcementId));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
}
#endif
