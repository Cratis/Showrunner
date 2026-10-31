// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Declaration.for_ProductOfferingBuilder.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_ProductOfferingBuilder.when_building;

public class a_module_that_requires_a_missing_module : an_offering_being_declared
{
    void Because()
    {
        _builder.Module("analytics", "Analytics", module => module.Requires("core"));
        Build();
    }

    [Fact] void should_refuse_the_offering() => _invalid.ShouldNotBeNull();
    [Fact] void should_name_the_module() => _invalid.Problems.ShouldContain(_ => _.Contains("'analytics'", StringComparison.Ordinal));
    [Fact] void should_name_the_missing_module() => _invalid.Problems.ShouldContain(_ => _.Contains("'core'", StringComparison.Ordinal));
    [Fact] void should_not_give_an_offering() => _offering.ShouldBeNull();
}
#endif
