// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.AuthorAnnouncement.when_authoring_an_announcement;

/// <summary>
/// A client that leaves the message out altogether, as the generated TypeScript command does when it is not set, still
/// authors the announcement, and it is kept with an empty message.
/// </summary>
public class with_the_message_left_out : an_announcement_command<AuthorAnnouncement>
{
    async Task Because() => _result = await _scenario.Execute(new AuthorAnnouncement(
        _announcementId,
        AnnouncementEvents.Product,
        Audience.Everyone(),
        "Maintenance tonight",
        null,
        Display.Banner,
        Importance.Warning,
        ActiveFrom.Immediately,
        ActiveUntil.Indefinitely));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_it_with_an_empty_message() =>
        await _scenario.ShouldHaveAppendedEvent<AuthorAnnouncement, AnnouncementAuthored>(_announcementId, e => e.Body == AnnouncementBody.NotSet);
}
#endif
