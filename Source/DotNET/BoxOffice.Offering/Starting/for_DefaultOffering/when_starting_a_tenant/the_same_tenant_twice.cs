// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Starting.for_DefaultOffering.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Starting.for_DefaultOffering.when_starting_a_tenant;

public class the_same_tenant_twice : a_product_with_a_default_offering
{
    TenantStart _second;
    int _count;

    async Task Because()
    {
        await StartingWith(Offering()).StartTenant(Tenant, Organization);
        _second = await StartingWith(Offering()).StartTenant(Tenant, Organization);
        _count = (await Told()).Count;
    }

    [Fact] void should_say_it_had_started() => _second.ShouldEqual(TenantStart.AlreadyStarted);
    [Fact] void should_tell_the_operator_once() => _count.ShouldEqual(1);
}
#endif
