// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Metering.for_MeteringProblems.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering.for_MeteringProblems;

public class when_a_meter_is_declared : an_offering_with_a_compute_meter
{
    IEnumerable<string> _problems;

    void Because() => _problems = MeteringProblems.In(_offering, [MeterFor("compute")]).ToArray();

    [Fact] void should_find_no_problems() => _problems.ShouldBeEmpty();
}
#endif
