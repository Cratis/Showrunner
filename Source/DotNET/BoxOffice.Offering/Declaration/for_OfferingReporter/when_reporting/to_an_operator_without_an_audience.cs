// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Declaration.for_OfferingReporter.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_OfferingReporter.when_reporting;

public class to_an_operator_without_an_audience : a_product_starting
{
    async Task Because()
    {
        await ReporterFor(CoreAndAnalytics).Report(_eventStore);
        await ReporterFor(builder => builder.DefaultOffering("Starter")).ReportDefaultOffering(_eventStore);
        await ReadReported();
    }

    [Fact] void should_have_reported_the_offering_and_the_default() => _reported.Count.ShouldEqual(2);
    [Fact] void should_not_address_the_offering() => _reported[0].Context.NamedTags.ShouldBeEmpty();
    [Fact] void should_not_address_the_default_offering() => _reported[1].Context.NamedTags.ShouldBeEmpty();
}
#endif
