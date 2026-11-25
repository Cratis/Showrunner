// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Arc.Availability;
using Cratis.Arc.Availability.Suspensions;
using Cratis.Showrunner.GuestList.Availability.Declaration;
using Cratis.Showrunner.GuestList.Availability.Declaration.for_AvailabilityReporter.given;
using Cratis.Showrunner.GuestList.Contracts.Availability;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.GuestList.Availability.Parts.for_PartsReporter.when_reporting;

/// <summary>
/// A product that has no parts reports nothing. Once it has some they are reported once, every module, feature, slice and
/// command whether the model declares anything or not, in the order of their paths whatever order the engine lists them in;
/// a start that finds the same parts reports nothing again, and a change, such as a part that is gone, is reported as the
/// whole list.
/// </summary>
public class parts_that_change : a_product_reporting
{
    static readonly PartEntry Module = new("module", "Fleet", "Fleet");
    static readonly PartEntry Feature = new("feature", "Fleet/Reports", "Reports");
    static readonly PartEntry Command = new("command", "Fleet/Reports/Export/Fleet.Reports.Export.ExportReport", "ExportReport");

    AvailabilityReport _nothing;
    AvailabilityReport _first;
    AvailabilityReport _again;
    AvailabilityReport _changed;

    async Task Because()
    {
        _nothing = await ReporterOf().Report(_eventStore);
        _first = await ReporterOf(Command, Module, Feature).Report(_eventStore);
        _again = await ReporterOf(Module, Feature, Command).Report(_eventStore);
        _changed = await ReporterOf(Module, Feature).Report(_eventStore);
        await ReadReported();
    }

    [Fact] void should_report_nothing_for_a_product_that_has_no_parts() => _nothing.ShouldEqual(AvailabilityReport.NothingDeclared);
    [Fact] void should_report_the_parts_once_they_are_known() => _first.ShouldEqual(AvailabilityReport.Reported);
    [Fact] void should_not_report_the_same_parts_in_another_order_again() => _again.ShouldEqual(AvailabilityReport.Unchanged);
    [Fact] void should_report_a_change() => _changed.ShouldEqual(AvailabilityReport.Reported);
    [Fact] void should_have_reported_twice() => _reported.Count.ShouldEqual(2);
    [Fact] void should_report_the_parts_in_the_order_of_their_paths() => Declared(0).Parts.Select(_ => _.Path.Value).ShouldEqual(["Fleet", "Fleet/Reports", "Fleet/Reports/Export/Fleet.Reports.Export.ExportReport"]);
    [Fact] void should_report_what_kind_each_is() => Declared(0).Parts.Select(_ => _.Kind).ShouldEqual(["module", "feature", "command"]);
    [Fact] void should_report_the_whole_list_again_without_the_part_that_is_gone() => Declared(1).Parts.Select(_ => _.Path.Value).ShouldEqual(["Fleet", "Fleet/Reports"]);

    static PartsReporter ReporterOf(params PartEntry[] parts) =>
        new(
            new InMemoryAvailabilityManifest(new AvailabilityManifest("Fleet/availability.screenplay", [], [], [], [], []) { Parts = parts }),
            Options.Create(new GuestListOptions { Product = "ledger", Environment = "Staging" }),
            new AvailabilityOptions(),
            NullLogger<PartsReporter>.Instance);

    PartsDeclared Declared(int index) => (PartsDeclared)_reported[index].Content;
}
#endif
