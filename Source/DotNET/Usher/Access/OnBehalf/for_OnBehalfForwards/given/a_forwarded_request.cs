// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Chronicle.Testing.Events;
using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Tenancy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfForwards.given;

/// <summary>
/// A request AuthProxy forwards in an on-behalf session on an issue reported in a tenant, with an assertion Arc
/// accepted: Arc's authentication answers with the reporter and the support person as its actor.
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
    protected SupportSessions _sessions;
    protected IAuthenticationService _authentication;
    protected bool _arcAccepts = true;
    protected string _actorSubject = "support-person";
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

        _sessions = new(_time, new RecordingLogger<SupportSessions>());
        _forwards = new(PassOn, AuthProxyTokens.ArcOptions(_authProxy), new RecordingLogger<OnBehalfForwards>());
        _authentication = Substitute.For<IAuthenticationService>();
        _authentication.AuthenticateAsync(Arg.Any<HttpContext>(), Arg.Any<string?>()).Returns(_ => Authenticated());

        var session = new OnBehalfSessionFacts((string)Reporters.SessionId, Tenant, _issueId.Value.ToString(), (string)Reporters.Subject, "support-person", "aad");
        _http = new DefaultHttpContext { RequestServices = new ServiceCollection().AddSingleton(_authentication).BuildServiceProvider() };
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

    protected Task Forwarded() => _forwards.InvokeAsync(_http, _stores, _sessions);

    AuthenticateResult Authenticated()
    {
        if (!_arcAccepts)
        {
            return AuthenticateResult.Fail("on-behalf assertion was not accepted");
        }

        var represented = new ClaimsIdentity([new Claim("sub", (string)Reporters.Subject)], "MicrosoftIdentityPlatform")
        {
            Actor = new ClaimsIdentity([new Claim("sub", _actorSubject)], "MicrosoftIdentityPlatform")
        };
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(represented), "MicrosoftIdentityPlatform"));
    }

    Task PassOn(HttpContext http)
    {
        _passedOn = true;
        return Task.CompletedTask;
    }
}
#endif
