// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Declaration.for_ProductOfferingBuilder.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_ProductOfferingBuilder.when_building;

public class modules_that_require_each_other : an_offering_being_declared
{
    void Because()
    {
        _builder
            .Module("a", "A", module => module.Requires("b"))
            .Module("b", "B", module => module.Requires("c"))
            .Module("c", "C", module => module.Requires("a"));
        Build();
    }

    [Fact] void should_refuse_the_offering() => _invalid.ShouldNotBeNull();
    [Fact] void should_report_the_circle_once() => _invalid.Problems.Count(_ => _.Contains("in a circle", StringComparison.Ordinal)).ShouldEqual(1);
}
#endif
