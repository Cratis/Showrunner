// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Availability.Declaration.for_AvailabilityReporter.given;

namespace Cratis.Showrunner.GuestList.Availability.Declaration.for_AvailabilityReporter.when_reporting;

/// <summary>
/// The product starts twice with the same model, as a restart does: the operator is told once, and what it is told says
/// what the manifest declares.
/// </summary>
public class the_same_declaration_on_two_starts : a_product_reporting
{
    AvailabilityReport _first;
    AvailabilityReport _second;

    async Task Because()
    {
        _first = await ReporterFor(PlusAndPreview).Report(_eventStore);
        _second = await ReporterFor(PlusAndPreview).Report(_eventStore);
        await ReadReported();
    }

    [Fact] void should_report_the_first_start() => _first.ShouldEqual(AvailabilityReport.Reported);
    [Fact] void should_not_report_the_second_start() => _second.ShouldEqual(AvailabilityReport.Unchanged);
    [Fact] void should_have_reported_once() => _reported.Count.ShouldEqual(1);
    [Fact] void should_report_for_the_product() => Declared().Product.Value.ShouldEqual("ledger");
    [Fact] void should_report_the_environment() => Declared().Environment.Value.ShouldEqual("Staging");
    [Fact] void should_report_the_version() => Declared().Version.Value.ShouldEqual("1.0.0");
    [Fact] void should_report_where_the_model_comes_from() => Declared().Origin.ShouldEqual("Ledger/availability.screenplay");
    [Fact] void should_report_the_audiences() => Declared().Audiences.Select(_ => _.Name.Value).ShouldEqual(["Plus", "Preview"]);
    [Fact] void should_report_what_the_audiences_are_for() => Declared().Audiences.First().Description.ShouldEqual("Plus customers");
    [Fact] void should_report_the_gate() => Declared().Gates.Select(_ => _.Target).ShouldEqual(["Billing"]);
    [Fact] void should_report_what_opens_the_gate() => Declared().Gates.Single().Alternatives.Single().Audiences.Select(_ => _.Value).ShouldEqual(["Plus"]);
    [Fact] void should_not_say_every_audience_is_granted() => Declared().EveryDeclaredAudienceGranted.ShouldBeFalse();
}
#endif
