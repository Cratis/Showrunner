// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_ProductOfferingBuilder.when_building;

public class a_default_offering_without_a_name : given.an_offering_being_declared
{
    void Because()
    {
        _builder.DefaultOffering(" ");
        Build();
    }

    [Fact] void should_refuse_the_offering() => _invalid.Problems.ShouldContain(_ => _.Contains("default offering has no name", StringComparison.Ordinal));
}
#endif
