// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Starting.for_DefaultOffering.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Starting.for_DefaultOffering.when_starting_a_tenant;

public class an_offering_the_operator_has_not_published : a_product_with_a_default_offering
{
    Exception _exception;
    int _count;
    int _entitled;

    async Task Because()
    {
        _exception = await Record.ExceptionAsync(() => StartingWith(Offering("Platinum")).StartTenant(Tenant, Organization));
        _count = (await Told()).Count;
        _entitled = (await Entitled()).Count;
    }

    [Fact] void should_fail_clearly() => Assert.IsType<DefaultOfferingNotPublished>(_exception);
    [Fact] void should_name_the_offering() => _exception.Message.ShouldContain("Platinum");
    [Fact] void should_tell_the_operator_nothing() => _count.ShouldEqual(0);
    [Fact] void should_entitle_the_tenant_to_nothing() => _entitled.ShouldEqual(0);
}
#endif
