// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;
using Cratis.Showrunner.Marquee.Receiving.for_MarqueeInbox.given;

namespace Cratis.Showrunner.Marquee.Receiving.for_MarqueeInbox.when_receiving;

/// <summary>
/// The authoring store serves several products: what is for another one is not recorded.
/// </summary>
public class an_announcement_for_another_product : a_marquee_inbox
{
    async Task Because()
    {
        await Delivered(ProductAnnouncements.Published(ProductAnnouncements.Banner(_announcementId), new Product("studio")));
        await Delivered(new AnnouncementRetracted(_announcementId, new Product("studio")));
        await ReadRecorded();
    }

    [Fact] void should_record_nothing() => _recorded.ShouldBeEmpty();
}
#endif
