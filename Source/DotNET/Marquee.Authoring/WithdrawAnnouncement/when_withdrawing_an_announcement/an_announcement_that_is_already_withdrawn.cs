// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.WithdrawAnnouncement.when_withdrawing_an_announcement;

/// <summary>
/// Withdrawing twice is refused, so the withdrawal is published once.
/// </summary>
public class an_announcement_that_is_already_withdrawn : an_announcement_command<WithdrawAnnouncement>
{
    void Establish() => Given(AnnouncementEvents.Maintenance, new AnnouncementWithdrawn());

    async Task Because() => _result = await _scenario.Execute(new WithdrawAnnouncement(_announcementId));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();

    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
