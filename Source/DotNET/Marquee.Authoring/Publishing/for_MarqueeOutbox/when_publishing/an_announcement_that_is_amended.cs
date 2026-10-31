// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;
using Cratis.Showrunner.Marquee.Authoring.Publishing.for_MarqueeOutbox.given;

namespace Cratis.Showrunner.Marquee.Authoring.Publishing.for_MarqueeOutbox.when_publishing;

/// <summary>
/// Every amendment is forwarded: the product revises what it shows.
/// </summary>
public class an_announcement_that_is_amended : a_marquee_outbox
{
    async Task Because()
    {
        await Recorded(AnnouncementEvents.Maintenance);
        await Happened(new AnnouncementAmended("Moved", "Later.", Importance.Critical, AnnouncementEvents.MaintenanceEnds, ActiveUntil.Indefinitely));
        await ReadPublished();
    }

    [Fact] void should_publish_that_it_was_revised() => _published.Single().Content.ShouldBeOfExactType<AnnouncementRevised>();

    [Fact]
    void should_publish_the_revision_for_the_product()
    {
        var revised = (AnnouncementRevised)_published.Single().Content;
        (revised.AnnouncementId, revised.Product, revised.Title, revised.Importance)
            .ShouldEqual((_announcementId, AnnouncementEvents.Product, new AnnouncementTitle("Moved"), Importance.Critical));
    }
}
#endif
