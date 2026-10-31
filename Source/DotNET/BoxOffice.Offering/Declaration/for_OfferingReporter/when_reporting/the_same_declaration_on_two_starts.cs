// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Declaration.for_OfferingReporter.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_OfferingReporter.when_reporting;

public class the_same_declaration_on_two_starts : a_product_starting
{
    OfferingReport _first;
    OfferingReport _second;

    async Task Because()
    {
        _first = await ReporterFor(CoreAndAnalytics).Report(_eventStore);
        _second = await ReporterFor(CoreAndAnalytics).Report(_eventStore);
        await ReadReported();
    }

    [Fact] void should_report_it_on_the_first_start() => _first.ShouldEqual(OfferingReport.Reported);
    [Fact] void should_not_report_it_on_the_second_start() => _second.ShouldEqual(OfferingReport.Unchanged);
    [Fact] void should_have_reported_once() => _reported.Count.ShouldEqual(1);
    [Fact] void should_report_under_the_product() => _reported[0].Context.EventSourceId.ShouldEqual((EventSourceId)_product);
    [Fact] void should_declare_the_modules() => ((OfferingDeclared)_reported[0].Content).Modules.Select(_ => _.Id.Value).ShouldContainOnly("core", "analytics");
    [Fact] void should_declare_the_meters() => ((OfferingDeclared)_reported[0].Content).Meters.Select(_ => _.Id.Value).ShouldContainOnly("compute");
}
#endif
