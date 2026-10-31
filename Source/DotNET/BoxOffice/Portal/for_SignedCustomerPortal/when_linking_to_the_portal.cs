// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Cryptography;
using System.Text;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Portal.for_SignedCustomerPortal.given;
using Cratis.Showrunner.BoxOffice.Seats;

namespace Cratis.Showrunner.BoxOffice.Portal.for_SignedCustomerPortal;

public class when_linking_to_the_portal : a_customer_portal
{
    static readonly Guid Offer = new(0x0d4f7d86, 0x6a1e, 0x4b2f, 0x9d, 0x0b, 0x3b, 0x8c, 0x7f, 0x6b, 0x2a, 0x11);

    string _link;
    System.Collections.Specialized.NameValueCollection _query;

    void Because()
    {
        _link = _portal.LinkFor(PortalIntent.ChangePlan, Offer, SeatHolder.Of(Tenants.Ada));
        _query = QueryOf(_link);
    }

    [Fact] void should_open_the_portal() => _link.StartsWith("https://portal.example.com/box-office?").ShouldBeTrue();
    [Fact] void should_carry_the_tenant() => _query["tenant"].ShouldEqual(_eventStore.Namespace.Value);
    [Fact] void should_carry_what_the_person_wants_to_do() => _query["intent"].ShouldEqual("changeplan");
    [Fact] void should_carry_the_offer() => _query["offer"].ShouldEqual(Offer.ToString("D"));
    [Fact] void should_carry_who_the_person_is() => _query["person"].ShouldEqual("github:1001");
    [Fact] void should_stop_working_after_the_lifetime() => _query["expires"].ShouldEqual((Tenants.Now + TimeSpan.FromMinutes(15)).ToUnixTimeSeconds().ToString());
    [Fact] void should_be_signed_the_way_the_portal_checks_it() => _query["signature"].ShouldEqual(ExpectedSignature());

    string ExpectedSignature()
    {
        var text = SignedCustomerPortal.Canonical(_eventStore.Namespace.Value, "changeplan", Offer.ToString("D"), "github:1001", long.Parse(_query["expires"]!));
        var signature = HMACSHA256.HashData(Encoding.UTF8.GetBytes(SigningKey), Encoding.UTF8.GetBytes(text));
        return Convert.ToBase64String(signature).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
#endif
