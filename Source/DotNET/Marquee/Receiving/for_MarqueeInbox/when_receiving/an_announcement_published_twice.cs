// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;
using Cratis.Showrunner.Marquee.Receiving.for_MarqueeInbox.given;

namespace Cratis.Showrunner.Marquee.Receiving.for_MarqueeInbox.when_receiving;

/// <summary>
/// A second publication of an announcement the product already holds is dropped.
/// </summary>
public class an_announcement_published_twice : a_marquee_inbox
{
    async Task Because()
    {
        await Delivered(Published());
        await Delivered(ProductAnnouncements.Published(ProductAnnouncements.DialogOnce(_announcementId)));
        await ReadRecorded();
    }

    [Fact] void should_record_one_fact() => _recorded.Count.ShouldEqual(1);

    [Fact] void should_keep_the_first() => ((AnnouncementReceived)_recorded.Single().Content).Title.ShouldEqual(new AnnouncementTitle("Scheduled maintenance"));
}
#endif
