// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Declaration.for_ProductOfferingBuilder.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_ProductOfferingBuilder.when_building;

public class a_module_that_requires_itself : an_offering_being_declared
{
    void Because()
    {
        _builder.Module("a", "A", module => module.Requires("a"));
        Build();
    }

    [Fact] void should_refuse_the_offering() => _invalid.Problems.ShouldContain(_ => _.Contains("requires itself", StringComparison.Ordinal));
}
#endif
