// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;
using Cratis.Showrunner.Marquee.Receiving.for_MarqueeInbox.given;

namespace Cratis.Showrunner.Marquee.Receiving.for_MarqueeInbox.when_receiving;

/// <summary>
/// A published announcement is recorded once, under its id in the product-wide stream.
/// </summary>
public class a_published_announcement : a_marquee_inbox
{
    async Task Because()
    {
        await Delivered(Published());
        await ReadRecorded();
    }

    [Fact] void should_record_one_fact() => _recorded.Count.ShouldEqual(1);

    [Fact] void should_record_that_it_was_received() => _recorded.Single().Content.ShouldBeOfExactType<AnnouncementReceived>();

    [Fact] void should_record_it_under_the_announcement_id() => _recorded.Single().Context.EventSourceId.ShouldEqual((EventSourceId)_announcementId);

    [Fact] void should_record_it_in_the_product_wide_stream() => _recorded.Single().Context.EventSourceType.ShouldEqual(MarqueeStream.SourceType);

    [Fact]
    void should_record_what_it_says()
    {
        var received = (AnnouncementReceived)_recorded.Single().Content;
        (received.Title, received.Display, received.ActiveFrom.Value, received.ActiveUntil.Value)
            .ShouldEqual((new AnnouncementTitle("Scheduled maintenance"), Display.Banner, ProductAnnouncements.MaintenanceStarts, ProductAnnouncements.MaintenanceEnds));
    }
}
#endif
