// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;
using Cratis.Showrunner.Marquee.Authoring.Publishing.for_MarqueeOutbox.given;

namespace Cratis.Showrunner.Marquee.Authoring.Publishing.for_MarqueeOutbox.when_publishing;

/// <summary>
/// Both amendments are forwarded, in order, not only the first.
/// </summary>
public class an_announcement_that_is_amended_twice : a_marquee_outbox
{
    async Task Because()
    {
        await Recorded(AnnouncementEvents.Maintenance);
        await Happened(new AnnouncementAmended("First", "Body.", Importance.Warning, AnnouncementEvents.MaintenanceStarts, AnnouncementEvents.MaintenanceEnds));
        await Happened(new AnnouncementAmended("Second", "Body.", Importance.Warning, AnnouncementEvents.MaintenanceStarts, AnnouncementEvents.MaintenanceEnds));
        await ReadPublished();
    }

    [Fact]
    void should_forward_both_in_order() =>
        _published.Select(published => ((AnnouncementRevised)published.Content).Title).ShouldEqual([new AnnouncementTitle("First"), new AnnouncementTitle("Second")]);
}
#endif
