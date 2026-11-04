// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Marquee.Authoring.given;
using Cratis.Showrunner.Marquee.Authoring.Publishing.for_MarqueeOutbox.given;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Marquee.Authoring.Publishing.for_MarqueeOutbox.when_publishing;

public class when_addressing_to_products : a_marquee_outbox
{
    async Task Because()
    {
        _outbox = new(_client, Substitute.For<ILogger<MarqueeOutbox>>(), Options.Create(new MarqueeAuthoringOptions { AddressToProducts = true }));
        await Happened(AnnouncementEvents.Maintenance);
        await ReadPublished();
    }

    [Fact] void should_address_the_announcement_to_its_product() => _published.Single().Context.NamedTags.ShouldContain(new NamedTag(WellKnownTags.Audience, ((AnnouncementPublished)_published.Single().Content).Product.Value));
}
#endif
