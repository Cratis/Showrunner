// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Declaration.for_OfferingReporter.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_OfferingReporter.when_reporting;

public class the_same_declaration_in_another_order : a_product_starting
{
    OfferingReport _second;

    async Task Because()
    {
        await ReporterFor(CoreAndAnalytics).Report(_eventStore);
        _second = await ReporterFor(builder => builder
            .Meter("compute", "Compute", "minutes")
            .Module("analytics", "Analytics", module => module.Requires("core"))
            .Module("core", "Core", module => module.Availability(ModuleAvailability.Included))).Report(_eventStore);
        await ReadReported();
    }

    [Fact] void should_not_report_it_again() => _second.ShouldEqual(OfferingReport.Unchanged);
    [Fact] void should_have_reported_once() => _reported.Count.ShouldEqual(1);
}
#endif
