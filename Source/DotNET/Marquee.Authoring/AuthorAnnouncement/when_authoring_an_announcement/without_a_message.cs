// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.AuthorAnnouncement.when_authoring_an_announcement;

/// <summary>
/// The message is optional: an announcement with only a title is accepted, and is kept with an empty message.
/// </summary>
public class without_a_message : an_announcement_command<AuthorAnnouncement>
{
    async Task Because() => _result = await _scenario.Execute(new AuthorAnnouncement(
        _announcementId,
        AnnouncementEvents.Product,
        Audience.Everyone(),
        "Maintenance tonight. [Read more here.](https://status.example.com)",
        AnnouncementBody.NotSet,
        Display.Banner,
        Importance.Warning,
        ActiveFrom.Immediately,
        ActiveUntil.Indefinitely));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact] void should_have_no_validation_errors() => _result.IsValid.ShouldBeTrue();

    [Fact]
    async Task should_append_it_with_an_empty_message() =>
        await _scenario.ShouldHaveAppendedEvent<AuthorAnnouncement, AnnouncementAuthored>(
            _announcementId,
            e => e.Body == AnnouncementBody.NotSet && e.Title == "Maintenance tonight. [Read more here.](https://status.example.com)");
}
#endif
