// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Collections.Immutable;
using System.Text;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Testing.Events;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.Activity;
using Cratis.Showrunner.BoxOffice.Product.Tenancy;
using Cratis.Showrunner.BoxOffice.Receiving;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.given;

public class a_sign_in_callback : Specification
{
    protected EventStoreForTesting _eventStore;
    protected BoxOfficeProductOptions _options;
    protected SignInCalls _calls;
    protected IImmutableList<AppendedEvent> _outbox;

    protected static string Body(string kind = "SignedIn", string subject = "1001", string? tenant = "fjord-logistics") =>
        $$"""{"kind":"{{kind}}","subject":"{{subject}}","name":"Ada Lovelace","email":"ada@fjord.example.com","identityProvider":"github"{{(tenant is null ? string.Empty : $$""","tenant":"{{tenant}}" """)}},"occurred":"2026-11-02T12:00:00+00:00"}""";

    protected static DefaultHttpContext Call(string body, string? marker = "SignedIn", string? principal = "1001", string? secret = null, string? tenantHeader = null)
    {
        var http = new DefaultHttpContext { RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider() };
        http.Request.Method = HttpMethods.Post;
        http.Request.Path = BoxOfficeProductOptions.DefaultSignInPath;
        http.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        if (marker is not null)
        {
            http.Request.Headers[SignInHeaders.Marker] = marker;
        }

        if (principal is not null)
        {
            http.Request.Headers[SignInHeaders.PrincipalId] = principal;
        }

        if (secret is not null)
        {
            http.Request.Headers[SignInHeaders.Secret] = secret;
        }

        if (tenantHeader is not null)
        {
            http.Request.Headers[SignInHeaders.Tenant] = tenantHeader;
        }

        return http;
    }

    void Establish()
    {
        _eventStore = new();
        _options = new();
        var stores = Substitute.For<ITenantStores>();
        stores.For(Tenants.Tenant, Arg.Any<CancellationToken>()).Returns(_eventStore);
        var activity = new ActivityReporter(stores, new Clock(Tenants.Now), Microsoft.Extensions.Logging.Abstractions.NullLogger<ActivityReporter>.Instance);
        _calls = new(activity, Options.Create(_options), Microsoft.Extensions.Logging.Abstractions.NullLogger<SignInCalls>.Instance);
        _eventStore.EventLog.Append(TenantStream.Id, new TenantOrganizationIdentified(Tenants.Organization)).GetAwaiter().GetResult();
    }

    void Destroy() => _eventStore.Dispose();

    protected async Task<int> Answer(DefaultHttpContext http)
    {
        var result = await _calls.Receive(http);
        await result.ExecuteAsync(http);
        return http.Response.StatusCode;
    }

    protected async Task ReadOutbox() =>
        _outbox = await _eventStore.GetEventSequence(EventSequenceId.Outbox).GetFromSequenceNumber(EventSequenceNumber.First);
}
#endif
