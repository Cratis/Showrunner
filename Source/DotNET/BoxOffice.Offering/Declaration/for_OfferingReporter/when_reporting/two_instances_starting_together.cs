// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Declaration.for_OfferingReporter.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_OfferingReporter.when_reporting;

public class two_instances_starting_together : a_product_starting
{
    async Task Because()
    {
        await Task.WhenAll(
            ReporterFor(CoreAndAnalytics).Report(_eventStore),
            ReporterFor(CoreAndAnalytics).Report(_eventStore));
        await ReadReported();
    }

    [Fact] void should_report_it_once() => _reported.Count.ShouldEqual(1);
}
#endif
