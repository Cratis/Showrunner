// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.RetargetAnnouncement.when_retargeting_an_announcement;

/// <summary>
/// Nothing changes once an announcement is withdrawn.
/// </summary>
public class a_withdrawn_announcement : an_announcement_command<RetargetAnnouncement>
{
    void Establish() => Given(AnnouncementEvents.Maintenance, new AnnouncementWithdrawn());

    async Task Because() => _result = await _scenario.Execute(new RetargetAnnouncement(_announcementId, Audience.Everyone()));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();

    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
