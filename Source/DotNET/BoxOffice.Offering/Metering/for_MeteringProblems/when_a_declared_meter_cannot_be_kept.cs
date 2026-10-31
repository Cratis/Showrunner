// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Metering.for_MeteringProblems.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering.for_MeteringProblems;

public class when_a_declared_meter_cannot_be_kept : Specification
{
    IEnumerable<string> _problems;

    void Because()
    {
        var offering = new Cratis.Showrunner.BoxOffice.Offering.Declaration.ProductOfferingBuilder()
            .Product(ProductId.New())
            .Meter("gpu.hours", "GPU", "hours")
            .Build();
        _problems = MeteringProblems.In(offering, []).ToArray();
    }

    [Fact] void should_find_one_problem() => _problems.Count().ShouldEqual(1);
}
#endif
