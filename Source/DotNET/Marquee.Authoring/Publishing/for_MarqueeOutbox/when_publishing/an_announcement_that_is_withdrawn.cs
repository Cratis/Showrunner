// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;
using Cratis.Showrunner.Marquee.Authoring.Publishing.for_MarqueeOutbox.given;

namespace Cratis.Showrunner.Marquee.Authoring.Publishing.for_MarqueeOutbox.when_publishing;

/// <summary>
/// Withdrawing publishes the retraction, for the product the announcement was authored for.
/// </summary>
public class an_announcement_that_is_withdrawn : a_marquee_outbox
{
    async Task Because()
    {
        await Recorded(AnnouncementEvents.Maintenance);
        await Happened(new AnnouncementWithdrawn());
        await ReadPublished();
    }

    [Fact] void should_publish_that_it_was_retracted() => _published.Single().Content.ShouldBeOfExactType<AnnouncementRetracted>();

    [Fact]
    void should_retract_it_for_the_product()
    {
        var retracted = (AnnouncementRetracted)_published.Single().Content;
        (retracted.AnnouncementId, retracted.Product).ShouldEqual((_announcementId, AnnouncementEvents.Product));
    }
}
#endif
