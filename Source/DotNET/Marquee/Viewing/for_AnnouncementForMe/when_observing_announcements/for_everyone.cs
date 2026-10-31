// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;

namespace Cratis.Showrunner.Marquee.Viewing.for_AnnouncementForMe.when_observing_announcements;

/// <summary>
/// An announcement for everyone reaches every tenant: nothing was copied to the tenant when it arrived.
/// </summary>
public class for_everyone : an_announcements_query
{
    readonly AnnouncementId _banner = AnnouncementId.New();
    readonly AnnouncementId _dialog = AnnouncementId.New();

    protected override IEnumerable<StandingAnnouncement> Held => [ProductAnnouncements.DialogOnce(_dialog), ProductAnnouncements.Banner(_banner)];

    async Task Because() => await Ask();

    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();

    [Fact] void should_return_both() => Announcements.Select(announcement => announcement.Id).ShouldEqual([_dialog, _banner]);

    [Fact] void should_return_the_announcements_that_start_first_first() => Announcements.First().Display.ShouldEqual(Display.DialogOnce);

    [Fact]
    void should_return_what_one_needs_to_show() =>
        Announcements.Single(announcement => announcement.Id == _banner).ActiveFrom.Value.ShouldEqual(ProductAnnouncements.MaintenanceStarts);
}
#endif
