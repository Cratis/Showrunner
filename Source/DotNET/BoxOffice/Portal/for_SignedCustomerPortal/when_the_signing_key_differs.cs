// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Portal.for_SignedCustomerPortal.given;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Portal.for_SignedCustomerPortal;

public class when_the_signing_key_differs : a_customer_portal
{
    string _signature;
    string _otherSignature;

    void Because()
    {
        _signature = QueryOf(_portal.LinkFor(PortalIntent.Buy, null, null))["signature"]!;
        var other = new SignedCustomerPortal(_eventStore, Options.Create(new BoxOfficeOptions { CustomerPortalUrl = _options.CustomerPortalUrl, CustomerPortalSigningKey = "another-key" }), _clock);
        _otherSignature = QueryOf(other.LinkFor(PortalIntent.Buy, null, null))["signature"]!;
    }

    [Fact] void should_sign_differently() => _signature.ShouldNotEqual(_otherSignature);
}
#endif
