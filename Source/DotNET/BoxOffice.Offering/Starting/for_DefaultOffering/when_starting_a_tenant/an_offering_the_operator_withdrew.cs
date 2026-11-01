// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Starting.for_DefaultOffering.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Starting.for_DefaultOffering.when_starting_a_tenant;

public class an_offering_the_operator_withdrew : a_product_with_a_default_offering
{
    Exception _exception;
    int _count;

    async Task Because()
    {
        _catalog[0] = _catalog[0] with { Withdrawn = true };
        _exception = await Record.ExceptionAsync(() => StartingWith(Offering()).StartTenant(Tenant, Organization));
        _count = (await Told()).Count;
    }

    [Fact] void should_fail_clearly() => Assert.IsType<DefaultOfferingNotPublished>(_exception);
    [Fact] void should_say_it_was_withdrawn() => _exception.Message.ShouldContain("withdrew");
    [Fact] void should_tell_the_operator_nothing() => _count.ShouldEqual(0);
}
#endif
