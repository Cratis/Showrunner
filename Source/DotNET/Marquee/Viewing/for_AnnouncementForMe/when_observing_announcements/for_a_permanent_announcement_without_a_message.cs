// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;

namespace Cratis.Showrunner.Marquee.Viewing.for_AnnouncementForMe.when_observing_announcements;

/// <summary>
/// An announcement with only a title and no window is returned as it is: nothing is filtered by date or by an empty message.
/// </summary>
public class for_a_permanent_announcement_without_a_message : an_announcements_query
{
    readonly AnnouncementId _banner = AnnouncementId.New();

    protected override IEnumerable<StandingAnnouncement> Held =>
    [
        ProductAnnouncements.Banner(_banner) with
        {
            Body = AnnouncementBody.NotSet,
            ActiveFrom = ActiveFrom.Immediately,
            ActiveUntil = ActiveUntil.Indefinitely
        }
    ];

    async Task Because() => await Ask();

    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();

    [Fact] void should_return_it() => Announcements.Select(announcement => announcement.Id).ShouldEqual([_banner]);

    [Fact] void should_have_no_message() => Announcements.Single().Body.ShouldEqual(AnnouncementBody.NotSet);

    [Fact] void should_start_immediately() => Announcements.Single().ActiveFrom.ShouldEqual(ActiveFrom.Immediately);

    [Fact] void should_have_no_end() => Announcements.Single().ActiveUntil.ShouldEqual(ActiveUntil.Indefinitely);
}
#endif
