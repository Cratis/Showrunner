// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Availability.Declaration.for_AvailabilityReporter.given;

namespace Cratis.Showrunner.GuestList.Availability.Declaration.for_AvailabilityReporter.when_reporting;

/// <summary>
/// The model gains an audience: the change is reported once, and a later start with that model reports nothing.
/// </summary>
public class a_declaration_that_changes : a_product_reporting
{
    AvailabilityReport _changed;
    AvailabilityReport _again;

    async Task Because()
    {
        await ReporterFor(PlusAndPreview).Report(_eventStore);
        var withInternal = Declares(["Plus", "Preview", "Internal"], "Billing", ["Plus"]);
        _changed = await ReporterFor(withInternal, "1.1.0").Report(_eventStore);
        _again = await ReporterFor(withInternal, "1.1.0").Report(_eventStore);
        await ReadReported();
    }

    [Fact] void should_report_the_change() => _changed.ShouldEqual(AvailabilityReport.Reported);
    [Fact] void should_not_report_it_again() => _again.ShouldEqual(AvailabilityReport.Unchanged);
    [Fact] void should_have_reported_twice() => _reported.Count.ShouldEqual(2);
    [Fact] void should_report_the_new_audience() => Declared(1).Audiences.Select(_ => _.Name.Value).ShouldContain("Internal");
    [Fact] void should_report_the_new_version() => Declared(1).Version.Value.ShouldEqual("1.1.0");
}
#endif
