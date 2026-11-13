// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Availability.Declaration.for_AvailabilityReporter.given;

namespace Cratis.Showrunner.GuestList.Availability.Declaration.for_AvailabilityReporter.when_reporting;

/// <summary>
/// Reordering what the model declares is not a change, and neither is a new build that declares the same.
/// </summary>
public class the_same_declaration_in_another_order : a_product_reporting
{
    AvailabilityReport _reordered;
    AvailabilityReport _newBuild;

    async Task Because()
    {
        await ReporterFor(Declares(["Plus", "Preview"], "Billing", ["Plus", "Preview"])).Report(_eventStore);
        _reordered = await ReporterFor(Declares(["Preview", "Plus"], "Billing", ["Preview", "Plus"])).Report(_eventStore);
        _newBuild = await ReporterFor(Declares(["Plus", "Preview"], "Billing", ["Plus", "Preview"]), "2.0.0").Report(_eventStore);
        await ReadReported();
    }

    [Fact] void should_not_report_the_reordering() => _reordered.ShouldEqual(AvailabilityReport.Unchanged);
    [Fact] void should_not_report_a_new_build_that_declares_the_same() => _newBuild.ShouldEqual(AvailabilityReport.Unchanged);
    [Fact] void should_have_reported_once() => _reported.Count.ShouldEqual(1);
}
#endif
