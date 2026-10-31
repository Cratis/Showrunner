// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;
using Cratis.Showrunner.Marquee.Authoring.Publishing.for_MarqueeOutbox.given;

namespace Cratis.Showrunner.Marquee.Authoring.Publishing.for_MarqueeOutbox.when_publishing;

/// <summary>
/// Retargeting publishes the new audience.
/// </summary>
public class an_announcement_that_is_retargeted : a_marquee_outbox
{
    async Task Because()
    {
        await Recorded(AnnouncementEvents.Maintenance);
        await Happened(new AnnouncementRetargeted(Audience.HoldersOfModule("reporting")));
        await ReadPublished();
    }

    [Fact] void should_publish_that_the_audience_changed() => _published.Single().Content.ShouldBeOfExactType<AnnouncementAudienceChanged>();

    [Fact]
    void should_publish_the_new_audience_for_the_product()
    {
        var changed = (AnnouncementAudienceChanged)_published.Single().Content;
        (changed.AnnouncementId, changed.Product, changed.Audience.Kind, changed.Audience.Module)
            .ShouldEqual((_announcementId, AnnouncementEvents.Product, AudienceKind.HoldersOfModule, new ModuleKey("reporting")));
    }
}
#endif
