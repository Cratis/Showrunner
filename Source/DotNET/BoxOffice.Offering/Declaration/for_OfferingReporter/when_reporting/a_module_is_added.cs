// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Declaration.for_OfferingReporter.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_OfferingReporter.when_reporting;

public class a_module_is_added : a_product_starting
{
    OfferingReport _second;

    async Task Because()
    {
        await ReporterFor(CoreAndAnalytics).Report(_eventStore);
        _second = await ReporterFor(builder =>
        {
            CoreAndAnalytics(builder);
            builder.Module("exports", "Exports", module => module.Requires("analytics"));
        }).Report(_eventStore);
        await ReadReported();
    }

    [Fact] void should_report_the_change() => _second.ShouldEqual(OfferingReport.Reported);
    [Fact] void should_have_reported_twice() => _reported.Count.ShouldEqual(2);
    [Fact] void should_declare_the_new_module() => ((OfferingDeclared)_reported[1].Content).Modules.Select(_ => _.Id.Value).ShouldContain("exports");
}
#endif
