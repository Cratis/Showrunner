// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.BoxOffice.Offering.Declaration.for_OfferingReporter.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_OfferingReporter.when_reporting;

public class to_an_operator_with_an_audience : a_product_starting
{
    async Task Because()
    {
        await ReporterFor(CoreAndAnalytics, "direct").Report(_eventStore);
        await ReporterFor(builder => builder.DefaultOffering("Starter"), "direct").ReportDefaultOffering(_eventStore);
        await ReadReported();
    }

    [Fact] void should_have_reported_the_offering_and_the_default() => _reported.Count.ShouldEqual(2);
    [Fact] void should_address_the_offering_to_the_operator() => _reported[0].Context.NamedTags.ShouldContain(new NamedTag(WellKnownTags.Audience, "direct"));
    [Fact] void should_address_the_default_offering_to_the_operator() => _reported[1].Context.NamedTags.ShouldContain(new NamedTag(WellKnownTags.Audience, "direct"));
}
#endif
