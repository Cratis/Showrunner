// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;
using Cratis.Showrunner.Marquee.Receiving;

namespace Cratis.Showrunner.Marquee.Standing.for_StandingAnnouncement.when_reducing;

/// <summary>
/// An announcement with no message and no window is held as it arrived: empty, shown immediately and never ending.
/// </summary>
public class a_permanent_announcement_without_a_message : a_read_model<StandingAnnouncement>
{
    StandingAnnouncement _announcement;

    async Task Because()
    {
        await Happened(ProductAnnouncements.Received(ProductAnnouncements.Banner(_announcementId) with
        {
            Body = AnnouncementBody.NotSet,
            ActiveFrom = ActiveFrom.Immediately,
            ActiveUntil = ActiveUntil.Indefinitely
        }));
        _announcement = _scenario.Instance;
    }

    [Fact] void should_be_held() => _announcement.Id.ShouldEqual(_announcementId);

    [Fact] void should_have_no_message() => _announcement.Body.ShouldEqual(AnnouncementBody.NotSet);

    [Fact] void should_start_immediately() => _announcement.ActiveFrom.ShouldEqual(ActiveFrom.Immediately);

    [Fact] void should_have_no_end() => _announcement.ActiveUntil.ShouldEqual(ActiveUntil.Indefinitely);
}
#endif
