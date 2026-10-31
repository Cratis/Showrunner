// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle.Testing.Events;
using Cratis.Showrunner.BoxOffice.given;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Portal.for_SignedCustomerPortal.given;

public class a_customer_portal : Specification
{
    protected const string SigningKey = "a-key-only-the-product-and-the-operator-know";

    protected EventStoreForTesting _eventStore;
    protected BoxOfficeOptions _options;
    protected Clock _clock;
    protected SignedCustomerPortal _portal;

    void Establish()
    {
        _eventStore = new();
        _clock = new(Tenants.Now);
        _options = new()
        {
            CustomerPortalUrl = new("https://portal.example.com/box-office"),
            CustomerPortalSigningKey = SigningKey
        };
        _portal = new(_eventStore, Options.Create(_options), _clock);
    }

    void Destroy() => _eventStore.Dispose();

    protected static System.Collections.Specialized.NameValueCollection QueryOf(string link) =>
        System.Web.HttpUtility.ParseQueryString(new Uri(link).Query);
}
#endif
