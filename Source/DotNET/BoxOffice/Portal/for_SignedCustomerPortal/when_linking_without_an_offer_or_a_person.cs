// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Portal.for_SignedCustomerPortal.given;

namespace Cratis.Showrunner.BoxOffice.Portal.for_SignedCustomerPortal;

public class when_linking_without_an_offer_or_a_person : a_customer_portal
{
    System.Collections.Specialized.NameValueCollection _query;
    string _otherSignature;

    void Because()
    {
        _query = QueryOf(_portal.LinkFor(PortalIntent.Buy, null, null));
        _otherSignature = QueryOf(_portal.LinkFor(PortalIntent.Try, null, null))["signature"]!;
    }

    [Fact] void should_carry_no_offer() => _query["offer"].ShouldBeNull();
    [Fact] void should_carry_no_person() => _query["person"].ShouldBeNull();
    [Fact] void should_carry_what_the_person_wants_to_do() => _query["intent"].ShouldEqual("buy");
    [Fact] void should_sign_what_the_person_wants_to_do() => _query["signature"].ShouldNotEqual(_otherSignature);
}
#endif
