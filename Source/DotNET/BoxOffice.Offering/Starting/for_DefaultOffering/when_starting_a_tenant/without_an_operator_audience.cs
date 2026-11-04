// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.BoxOffice.Offering.Starting.for_DefaultOffering.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Starting.for_DefaultOffering.when_starting_a_tenant;

public class without_an_operator_audience : a_product_with_a_default_offering
{
    NamedTag[] _told;

    async Task Because()
    {
        await StartingWith(Offering()).StartTenant(Tenant, Organization);
        _told = [.. (await Told()).Single().Context.NamedTags];
    }

    [Fact] void should_not_address_what_the_operator_is_told() => _told.ShouldBeEmpty();
}
#endif
