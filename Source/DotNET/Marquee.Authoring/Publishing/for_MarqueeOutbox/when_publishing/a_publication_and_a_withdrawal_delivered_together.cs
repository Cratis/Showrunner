// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;
using Cratis.Showrunner.Marquee.Authoring.Publishing.for_MarqueeOutbox.given;

namespace Cratis.Showrunner.Marquee.Authoring.Publishing.for_MarqueeOutbox.when_publishing;

/// <summary>
/// Delivered together, as after the reactor was down, the announcement is never left standing: its publication is not shown and its withdrawal is published.
/// </summary>
public class a_publication_and_a_withdrawal_delivered_together : a_marquee_outbox
{
    async Task Because()
    {
        await Recorded(AnnouncementEvents.Maintenance, new AnnouncementWithdrawn());
        await DeliverEverything();
        await ReadPublished();
    }

    [Fact] void should_end_with_the_retraction() => _published[^1].Content.ShouldBeOfExactType<AnnouncementRetracted>();

    [Fact] void should_not_publish_it_as_standing() => _published.Select(published => published.Content).OfType<AnnouncementPublished>().ShouldBeEmpty();
}
#endif
