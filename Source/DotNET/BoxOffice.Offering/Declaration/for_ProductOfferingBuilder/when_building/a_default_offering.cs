// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_ProductOfferingBuilder.when_building;

public class a_default_offering : given.an_offering_being_declared
{
    void Because()
    {
        _builder.DefaultOffering("Starter");
        Build();
    }

    [Fact] void should_build_the_offering() => _invalid.ShouldBeNull();
    [Fact] void should_name_the_default_offer() => _offering.DefaultOffer!.Value.ShouldEqual("Starter");
}
#endif
