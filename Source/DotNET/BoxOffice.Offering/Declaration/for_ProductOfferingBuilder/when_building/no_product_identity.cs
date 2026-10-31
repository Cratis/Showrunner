// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_ProductOfferingBuilder.when_building;

public class no_product_identity : Specification
{
    InvalidOffering _invalid;

    void Because()
    {
        try
        {
            new ProductOfferingBuilder().Build();
        }
        catch (InvalidOffering invalid)
        {
            _invalid = invalid;
        }
    }

    [Fact] void should_refuse_the_offering() => _invalid.Problems.ShouldContain(_ => _.Contains("no identity", StringComparison.Ordinal));
}
#endif
