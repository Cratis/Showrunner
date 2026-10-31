// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Declaration.for_ProductOfferingBuilder.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_ProductOfferingBuilder.when_building;

public class a_namespace_owned_by_two_modules : an_offering_being_declared
{
    void Because()
    {
        _builder
            .Module("a", "A", module => module.Owns("Acme.Shared"))
            .Module("b", "B", module => module.Owns("Acme.Shared"));
        Build();
    }

    [Fact] void should_refuse_the_offering() => _invalid.Problems.ShouldContain(_ => _.Contains("'Acme.Shared' is owned by more than one module", StringComparison.Ordinal));
}
#endif
