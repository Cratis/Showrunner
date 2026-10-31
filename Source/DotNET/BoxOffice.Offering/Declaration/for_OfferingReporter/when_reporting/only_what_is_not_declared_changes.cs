// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Declaration.for_OfferingReporter.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_OfferingReporter.when_reporting;

public class only_what_is_not_declared_changes : a_product_starting
{
    OfferingReport _second;

    async Task Because()
    {
        await ReporterFor(CoreAndAnalytics).Report(_eventStore);

        // Trial days, what a module requires and what it owns are not part of what the operator is told.
        _second = await ReporterFor(builder => builder
            .Module("core", "Core", module => module.Availability(ModuleAvailability.Included).Owns("Acme.Core"))
            .Module("analytics", "Analytics", module => module.Requires("core").TrialDays(30))
            .Meter("compute", "Compute", "minutes", hasConcurrencyLimit: true)).Report(_eventStore);
        await ReadReported();
    }

    [Fact] void should_not_report_it_again() => _second.ShouldEqual(OfferingReport.Unchanged);
    [Fact] void should_have_reported_once() => _reported.Count.ShouldEqual(1);
}
#endif
