// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Declaration.for_ProductOfferingBuilder.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_ProductOfferingBuilder.when_building;

public class meters_with_the_same_key_and_no_unit : an_offering_being_declared
{
    void Because()
    {
        _builder.Meter("compute", "Compute", "minutes").Meter("compute", "Compute again", string.Empty);
        Build();
    }

    [Fact] void should_refuse_the_duplicate() => _invalid.Problems.ShouldContain(_ => _.Contains("Meter 'compute' is declared 2 times", StringComparison.Ordinal));
    [Fact] void should_refuse_the_missing_unit() => _invalid.Problems.ShouldContain(_ => _.Contains("counts in no unit", StringComparison.Ordinal));
}
#endif
