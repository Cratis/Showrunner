// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;
using Cratis.Showrunner.Marquee.Authoring.Publishing.for_MarqueeOutbox.given;

namespace Cratis.Showrunner.Marquee.Authoring.Publishing.for_MarqueeOutbox.when_publishing;

/// <summary>
/// Authoring publishes the announcement, with everything the product needs to show it.
/// </summary>
public class an_announcement_that_is_authored : a_marquee_outbox
{
    async Task Because()
    {
        await Happened(AnnouncementEvents.Maintenance);
        await ReadPublished();
    }

    [Fact] void should_publish_one_event() => _published.Count.ShouldEqual(1);

    [Fact] void should_publish_that_it_was_published() => _published.Single().Content.ShouldBeOfExactType<AnnouncementPublished>();

    [Fact]
    void should_publish_it_with_the_id_the_product_and_the_window()
    {
        var published = (AnnouncementPublished)_published.Single().Content;
        (published.AnnouncementId, published.Product, published.Title, published.Display, published.ActiveFrom.Value, published.ActiveUntil.Value)
            .ShouldEqual((_announcementId, AnnouncementEvents.Product, new AnnouncementTitle("Scheduled maintenance"), Display.Banner, AnnouncementEvents.MaintenanceStarts, AnnouncementEvents.MaintenanceEnds));
    }

    [Fact]
    void should_publish_it_under_the_product_so_its_announcements_share_one_stream() =>
        _published.Single().Context.EventSourceId.ShouldEqual(new EventSourceId("ledger"));
}
#endif
