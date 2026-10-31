// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Portal.for_SignedCustomerPortal.given;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Portal.for_SignedCustomerPortal;

public class when_the_product_has_no_portal : a_customer_portal
{
    SignedCustomerPortal _without;
    Exception _exception;

    void Because()
    {
        _without = new(_eventStore, Options.Create(new BoxOfficeOptions()), _clock);
        _exception = Catch.Exception(() => _without.LinkFor(PortalIntent.Buy, null, null));
    }

    [Fact] void should_not_be_available() => _without.IsAvailable.ShouldBeFalse();
    [Fact] void should_refuse_to_link() => _exception.ShouldBeOfExactType<CustomerPortalNotConfigured>();
}
#endif
