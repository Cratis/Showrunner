// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.AmendAnnouncement.when_amending_an_announcement;

/// <summary>
/// The message and the window are optional: amending can take the message away and make the announcement permanent.
/// </summary>
public class without_a_message_or_a_window : an_announcement_command<AmendAnnouncement>
{
    void Establish() => Given(AnnouncementEvents.Maintenance);

    async Task Because() => _result = await _scenario.Execute(new AmendAnnouncement(
        _announcementId,
        "Maintenance",
        AnnouncementBody.NotSet,
        Importance.Warning,
        new ActiveFrom(default),
        new ActiveUntil(default)));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_it_with_an_empty_message_that_starts_immediately_and_never_ends() =>
        await _scenario.ShouldHaveAppendedEvent<AmendAnnouncement, AnnouncementAmended>(
            _announcementId,
            e => e.Body == AnnouncementBody.NotSet &&
                 e.ActiveFrom == ActiveFrom.Immediately &&
                 e.ActiveUntil == ActiveUntil.Indefinitely);
}
#endif
