// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Metering.for_MeteringProblems.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering.for_MeteringProblems;

public class when_a_meter_reads_no_events : an_offering_with_a_compute_meter
{
    IEnumerable<string> _problems;

    void Because()
    {
        var meter = Substitute.For<IMeter>();
        meter.Id.Returns(new MeterId("compute"));
        meter.EventTypes.Returns([]);
        _problems = MeteringProblems.In(_offering, [meter]).ToArray();
    }

    [Fact] void should_find_one_problem() => _problems.Count().ShouldEqual(1);
}
#endif
