// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Starting.for_DefaultOffering.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Starting.for_DefaultOffering.when_starting_a_tenant;

public class a_product_without_a_default_offering : a_product_with_a_default_offering
{
    Exception _exception;

    async Task Because() => _exception = await Record.ExceptionAsync(() => StartingWith(Offering(null)).StartTenant(Tenant, Organization));

    [Fact] void should_fail_clearly() => Assert.IsType<DefaultOfferingNotDeclared>(_exception);
}
#endif
