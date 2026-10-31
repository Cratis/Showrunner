// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.AuthorAnnouncement.when_authoring_an_announcement;

/// <summary>
/// An announcement without a window is shown immediately and indefinitely.
/// </summary>
public class without_a_window : an_announcement_command<AuthorAnnouncement>
{
    async Task Because() => _result = await _scenario.Execute(new AuthorAnnouncement(
        _announcementId,
        AnnouncementEvents.Product,
        Audience.Everyone(),
        "Meet Reporting",
        "Reports on everything.",
        Display.DialogOnce,
        Importance.Information,
        new ActiveFrom(default),
        new ActiveUntil(default)));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_start_immediately_and_never_end() =>
        await _scenario.ShouldHaveAppendedEvent<AuthorAnnouncement, AnnouncementAuthored>(_announcementId,
            e => e.Display == Display.DialogOnce &&
                 e.ActiveFrom == ActiveFrom.Immediately &&
                 e.ActiveUntil == ActiveUntil.Indefinitely);
}
#endif
