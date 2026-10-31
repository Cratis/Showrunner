// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;
using Cratis.Showrunner.BoxOffice.Catalog.Publishing.for_OfferPublisher.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Publishing.for_OfferPublisher.when_publishing;

public class a_published_offer_that_includes_a_module : an_offer_publisher
{
    OfferPublished _published;

    async Task Because()
    {
        await Recorded(OfferEvents.Team, OfferEvents.Priced);
        await Happened(new OfferIncludedModule("reporting"));
        await ReadOutbox();
        _published = _outbox.Single().Content as OfferPublished;
    }

    [Fact] void should_publish_the_offer_again() => _published.ShouldNotBeNull();
    [Fact] void should_include_the_module() => _published.Modules.ShouldEqual([new ModuleId("reporting")]);
}
#endif
