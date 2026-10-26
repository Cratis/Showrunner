// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle.Testing.Events;
using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfForwards.given;

/// <summary>
/// A request AuthProxy forwards in an on-behalf session on an issue reported in a tenant, with a valid assertion.
/// </summary>
public class a_forwarded_request : Specification
{
    protected const string Tenant = "tenant-one";
    protected const string PrincipalHeader = "eyJ1c2VySWQiOiJyZXBvcnRlciJ9";
    protected const string ActorHeader = "eyJ1c2VySWQiOiJzdXBwb3J0In0=";
    protected static readonly DateTimeOffset Now = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);

    protected AuthProxyTokens _authProxy;
    protected ScenarioTime _time;
    protected EventStoreForTesting _eventStore;
    protected ITenantEventStores _stores;
    protected OnBehalfForwards _forwards;
    protected OnBehalfTokens _tokens;
    protected SupportSessions _sessions;
    protected IssueId _issueId;
    protected DefaultHttpContext _http;
    protected bool _passedOn;

    protected VerifiedForward? Forward => _http.Items.TryGetValue(OnBehalfForwards.VerifiedForward, out var forward) ? forward as VerifiedForward : null;

    void Establish()
    {
        _authProxy = new();
        _time = new(Now);
        _eventStore = new();
        _issueId = IssueId.New();
        _stores = Substitute.For<ITenantEventStores>();
        _stores.For(new TenantId(Tenant), Arg.Any<CancellationToken>()).Returns(_eventStore);

        var options = Substitute.For<IOptionsMonitor<UsherOptions>>();
        options.CurrentValue.Returns(new UsherOptions
        {
            ApplicationName = "Ledger",
            OnBehalf = new()
            {
                Issuer = AuthProxyTokens.Issuer,
                Audience = AuthProxyTokens.Audience,
                SigningKeys = [new() { KeyId = AuthProxyTokens.KeyId, PublicKeyPem = _authProxy.PublicKeyPem }]
            }
        });
        _tokens = new(options, new OnBehalfTokenReplays(_time), _time);
        _sessions = new(_time, new RecordingLogger<SupportSessions>());
        _forwards = new(PassOn, options, new RecordingLogger<OnBehalfForwards>());

        var session = new OnBehalfSessionFacts((string)Reporters.SessionId, Tenant, _issueId.Value.ToString(), (string)Reporters.Subject, "support-person", "aad");
        _http = new DefaultHttpContext();
        _http.Request.Path = "/api/issues";
        _http.Request.Headers[OnBehalfPaths.PrincipalHeader] = PrincipalHeader;
        _http.Request.Headers[OnBehalfPaths.ActorHeader] = ActorHeader;
        _http.Request.Headers[OnBehalfPaths.TenantHeader] = Tenant;
        _http.Request.Headers[OnBehalfPaths.AssertionHeader] = _authProxy.ForForward(session, PrincipalHeader, ActorHeader, Now);
    }

    void Destroy()
    {
        _eventStore.Dispose();
        _authProxy.Dispose();
    }

    protected async Task Happened(params object[] events)
    {
        foreach (var @event in events)
        {
            await _eventStore.EventLog.Append(_issueId, @event);
        }
    }

    protected Task Forwarded() => _forwards.InvokeAsync(_http, _tokens, _stores, _sessions);

    Task PassOn(HttpContext http)
    {
        _passedOn = true;
        return Task.CompletedTask;
    }
}
#endif
