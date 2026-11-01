// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Declaration.for_OfferingReporter.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_OfferingReporter.when_reporting;

public class a_default_offering : a_product_starting
{
    OfferingReport _first;
    OfferingReport _again;
    OfferingReport _changed;
    OfferingReport _none;

    async Task Because()
    {
        _first = await ReporterFor(builder => builder.DefaultOffering("Starter")).ReportDefaultOffering(_eventStore);
        _again = await ReporterFor(builder => builder.DefaultOffering("Starter")).ReportDefaultOffering(_eventStore);
        _changed = await ReporterFor(builder => builder.DefaultOffering("Team")).ReportDefaultOffering(_eventStore);
        _none = await ReporterFor(_ => { }).ReportDefaultOffering(_eventStore);
        await ReadReported();
    }

    [Fact] void should_report_it() => _first.ShouldEqual(OfferingReport.Reported);
    [Fact] void should_not_report_it_again() => _again.ShouldEqual(OfferingReport.Unchanged);
    [Fact] void should_report_a_change() => _changed.ShouldEqual(OfferingReport.Reported);
    [Fact] void should_report_nothing_without_a_default() => _none.ShouldEqual(OfferingReport.Unchanged);
    [Fact] void should_have_reported_twice() => _reported.Count.ShouldEqual(2);
    [Fact] void should_declare_the_latest() => ((DefaultOfferingDeclared)_reported[1].Content).Offer.Value.ShouldEqual("Team");
}
#endif
