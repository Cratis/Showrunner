// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;
using Cratis.Showrunner.Marquee.Authoring.Publishing.for_MarqueeOutbox.given;

namespace Cratis.Showrunner.Marquee.Authoring.Publishing.for_MarqueeOutbox.when_publishing;

/// <summary>
/// Published when it happened, retracted after: the last word is the retraction.
/// </summary>
public class a_publication_then_a_withdrawal_delivered_in_order : a_marquee_outbox
{
    async Task Because()
    {
        await Happened(AnnouncementEvents.Maintenance);
        await Happened(new AnnouncementWithdrawn());
        await ReadPublished();
    }

    [Fact] void should_publish_the_publication_then_the_retraction() => _published.Select(published => published.Content.GetType()).ShouldEqual([typeof(AnnouncementPublished), typeof(AnnouncementRetracted)]);

    [Fact] void should_end_with_the_retraction() => _published[^1].Content.ShouldBeOfExactType<AnnouncementRetracted>();
}
#endif
