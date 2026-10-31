// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;
using Cratis.Showrunner.Marquee.Authoring.Publishing.for_MarqueeOutbox.given;

namespace Cratis.Showrunner.Marquee.Authoring.Publishing.for_MarqueeOutbox.when_publishing;

/// <summary>
/// An amendment that is delivered late, after the announcement was withdrawn, is not published: it would show the announcement again.
/// </summary>
public class an_amendment_delivered_after_the_withdrawal : a_marquee_outbox
{
    async Task Because()
    {
        await Recorded(AnnouncementEvents.Maintenance);
        await Happened(new AnnouncementWithdrawn());
        await Recorded(new AnnouncementAmended("Late", "Late.", Importance.Warning, AnnouncementEvents.MaintenanceStarts, AnnouncementEvents.MaintenanceEnds));
        await DeliverEverything();
        await ReadPublished();
    }

    [Fact] void should_not_publish_a_revision() => _published.Select(published => published.Content).OfType<AnnouncementRevised>().ShouldBeEmpty();

    [Fact] void should_end_with_the_retraction() => _published[^1].Content.ShouldBeOfExactType<AnnouncementRetracted>();
}
#endif
