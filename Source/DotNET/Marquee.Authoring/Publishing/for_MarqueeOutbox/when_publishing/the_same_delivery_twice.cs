// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;
using Cratis.Showrunner.Marquee.Authoring.Publishing.for_MarqueeOutbox.given;

namespace Cratis.Showrunner.Marquee.Authoring.Publishing.for_MarqueeOutbox.when_publishing;

/// <summary>
/// A redelivery appends nothing a second time.
/// </summary>
public class the_same_delivery_twice : a_marquee_outbox
{
    async Task Because()
    {
        await Recorded(AnnouncementEvents.Maintenance, new AnnouncementAmended("Moved", "Later.", Importance.Critical, AnnouncementEvents.MaintenanceEnds, ActiveUntil.Indefinitely));
        await DeliverEverything();
        await DeliverEverything();
        await ReadPublished();
    }

    [Fact] void should_publish_each_fact_once() => _published.Count.ShouldEqual(2);
}
#endif
