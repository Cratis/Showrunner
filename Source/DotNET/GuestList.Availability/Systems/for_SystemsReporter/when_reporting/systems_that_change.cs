// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Choreography.Availability;
using Cratis.Showrunner.GuestList.Availability.Declaration;
using Cratis.Showrunner.GuestList.Availability.Declaration.for_AvailabilityReporter.given;
using Cratis.Showrunner.GuestList.Contracts.Audiences;
using Cratis.Showrunner.GuestList.Contracts.Availability;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.GuestList.Availability.Systems.for_SystemsReporter.when_reporting;

/// <summary>
/// A product that knows no systems reports nothing. Once it knows some they are reported once, in the order of their client
/// ids whatever order it lists them in, a start that knows the same reports nothing again, and a change, such as a system
/// that is gone, is reported as the whole list.
/// </summary>
public class systems_that_change : a_product_reporting
{
    static readonly SystemDeclaration Crm = new("crm-sync", SystemKind.Api, "CRM sync");
    static readonly SystemDeclaration Reports = new("reports-mcp", SystemKind.Mcp, "Reports assistant");

    AvailabilityReport _nothing;
    AvailabilityReport _first;
    AvailabilityReport _again;
    AvailabilityReport _changed;

    async Task Because()
    {
        _nothing = await ReporterOf().Report(_eventStore);
        _first = await ReporterOf(Reports, Crm).Report(_eventStore);
        _again = await ReporterOf(Crm, Reports).Report(_eventStore);
        _changed = await ReporterOf(Crm).Report(_eventStore);
        await ReadReported();
    }

    [Fact] void should_report_nothing_for_a_product_that_knows_no_systems() => _nothing.ShouldEqual(AvailabilityReport.NothingDeclared);
    [Fact] void should_report_the_systems_once_they_are_known() => _first.ShouldEqual(AvailabilityReport.Reported);
    [Fact] void should_not_report_the_same_systems_in_another_order_again() => _again.ShouldEqual(AvailabilityReport.Unchanged);
    [Fact] void should_report_a_change() => _changed.ShouldEqual(AvailabilityReport.Reported);
    [Fact] void should_have_reported_twice() => _reported.Count.ShouldEqual(2);
    [Fact] void should_report_the_systems_in_the_order_of_their_client_ids() => Declared(0).Systems.Select(_ => _.ClientId.Value).ShouldEqual(["crm-sync", "reports-mcp"]);
    [Fact] void should_report_what_kind_each_is() => Declared(0).Systems.Select(_ => _.Kind).ShouldEqual([SystemKind.Api, SystemKind.Mcp]);
    [Fact] void should_report_the_whole_list_again_without_the_system_that_is_gone() => Declared(1).Systems.Select(_ => _.ClientId.Value).ShouldEqual(["crm-sync"]);

    static SystemsReporter ReporterOf(params SystemDeclaration[] systems)
    {
        var known = new InMemoryKnownSystems();
        known.Set(systems);
        return new(known, Options.Create(new GuestListOptions { Product = "ledger", Environment = "Staging" }), new AvailabilityOptions(), NullLogger<SystemsReporter>.Instance);
    }

    SystemsDeclared Declared(int index) => (SystemsDeclared)_reported[index].Content;
}
#endif
