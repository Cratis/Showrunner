// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Availability.Declaration.for_AvailabilityReporter.given;
using Manifest = Cratis.Choreography.Availability;

namespace Cratis.Showrunner.GuestList.Availability.Declaration.for_AvailabilityReporter.when_reporting;

/// <summary>
/// A product without a model has nothing to say, so it says nothing.
/// </summary>
public class a_product_that_declares_nothing : a_product_reporting
{
    AvailabilityReport _report;

    async Task Because()
    {
        _report = await ReporterFor(Manifest.AvailabilityManifest.Empty).Report(_eventStore);
        await ReadReported();
    }

    [Fact] void should_have_nothing_to_report() => _report.ShouldEqual(AvailabilityReport.NothingDeclared);
    [Fact] void should_report_nothing() => _reported.Count.ShouldEqual(0);
}
#endif
