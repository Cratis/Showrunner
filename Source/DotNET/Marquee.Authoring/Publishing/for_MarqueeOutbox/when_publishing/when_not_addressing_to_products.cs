// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Marquee.Authoring.given;
using Cratis.Showrunner.Marquee.Authoring.Publishing.for_MarqueeOutbox.given;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Marquee.Authoring.Publishing.for_MarqueeOutbox.when_publishing;

public class when_not_addressing_to_products : a_marquee_outbox
{
    async Task Because()
    {
        _outbox = new(_client, Substitute.For<ILogger<MarqueeOutbox>>(), Options.Create(new MarqueeAuthoringOptions { AddressToProducts = false }));
        await Happened(AnnouncementEvents.Maintenance);
        await ReadPublished();
    }

    [Fact] void should_not_address_the_announcement() => _published.Single().Context.NamedTags.ShouldBeEmpty();
}
#endif
