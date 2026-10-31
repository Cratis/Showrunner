// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Declaration.for_OfferingReporter.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_OfferingReporter.when_reporting;

public class a_change_that_is_changed_back : a_product_starting
{
    async Task Because()
    {
        await ReporterFor(CoreAndAnalytics).Report(_eventStore);
        await ReporterFor(builder => builder.Module("core", "Core")).Report(_eventStore);
        await ReporterFor(CoreAndAnalytics).Report(_eventStore);
        await ReadReported();
    }

    [Fact] void should_report_every_change() => _reported.Count.ShouldEqual(3);
    [Fact] void should_end_with_what_the_product_declares_now() => ((OfferingDeclared)_reported[2].Content).Modules.Select(_ => _.Id.Value).ShouldContainOnly("core", "analytics");
}
#endif
