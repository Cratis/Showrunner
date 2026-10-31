// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;
using Cratis.Showrunner.Marquee.Receiving;

namespace Cratis.Showrunner.Marquee.Standing.for_StandingAnnouncement.when_reducing;

/// <summary>
/// A retraction removes the announcement: it is held no more.
/// </summary>
public class a_retracted_announcement : a_read_model<StandingAnnouncement>
{
    StandingAnnouncement _announcement;

    async Task Because()
    {
        await Happened(ProductAnnouncements.Received(ProductAnnouncements.DialogOnce(_announcementId)), new AnnouncementRetractionReceived());
        _announcement = _scenario.Instance;
    }

    [Fact] void should_no_longer_be_held() => _announcement.ShouldBeNull();
}
#endif
