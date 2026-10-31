// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.WithdrawAnnouncement.when_withdrawing_an_announcement;

/// <summary>
/// Withdrawing appends that the announcement was withdrawn.
/// </summary>
public class a_standing_announcement : an_announcement_command<WithdrawAnnouncement>
{
    void Establish() => Given(AnnouncementEvents.Maintenance);

    async Task Because() => _result = await _scenario.Execute(new WithdrawAnnouncement(_announcementId));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_that_it_was_withdrawn() =>
        await _scenario.ShouldHaveAppendedEvent<WithdrawAnnouncement, AnnouncementWithdrawn>(_announcementId);
}
#endif
