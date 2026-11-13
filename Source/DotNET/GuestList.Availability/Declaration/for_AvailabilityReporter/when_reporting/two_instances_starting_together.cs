// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Availability.Declaration.for_AvailabilityReporter.given;

namespace Cratis.Showrunner.GuestList.Availability.Declaration.for_AvailabilityReporter.when_reporting;

/// <summary>
/// Two instances of the product start together with the same model: the operator is told once.
/// </summary>
public class two_instances_starting_together : a_product_reporting
{
    async Task Because()
    {
        await Task.WhenAll(ReporterFor(PlusAndPreview).Report(_eventStore), ReporterFor(PlusAndPreview).Report(_eventStore));
        await ReadReported();
    }

    [Fact] void should_have_reported_once() => _reported.Count.ShouldEqual(1);
}
#endif
