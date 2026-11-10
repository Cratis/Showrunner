// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfForwards.given;
using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.given;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfForwards;

public class when_handling_other_forward_branches : a_forwarded_request
{
    readonly Dictionary<string, (bool Passed, int Status, string Cache, VerifiedForward? Forward)> _outcomes = new(StringComparer.Ordinal);

    async Task Because()
    {
        foreach (var scenario in new[] { "disabled", "ordinary", "callback", "no_actor", "missing_assertion", "invalid_assertion", "wrong_subject", "invalid_issue", "unknown_tenant", "name_identifier" })
        {
            var services = _http.RequestServices;
            _http = new DefaultHttpContext { RequestServices = services };
            _http.Request.Path = scenario == "callback" ? OnBehalfPaths.Grants : "/api/issues";
            _http.Request.Headers[OnBehalfPaths.TenantHeader] = scenario == "unknown_tenant" ? "unknown-tenant" : Tenant;
            if (scenario != "ordinary")
            {
                _http.Request.Headers[OnBehalfPaths.ActorHeader] = ActorHeader;
                var facts = new OnBehalfSessionFacts((string)Reporters.SessionId, scenario == "unknown_tenant" ? "unknown-tenant" : Tenant, scenario == "invalid_issue" ? "not-an-issue" : _issueId.Value.ToString(), scenario == "wrong_subject" ? "another-subject" : (string)Reporters.Subject, "support-person", "aad");
                if (scenario != "missing_assertion")
                {
                    _http.Request.Headers[OnBehalfPaths.AssertionHeader] = scenario == "invalid_assertion" ? "invalid" : _authProxy.ForForward(facts, PrincipalHeader, ActorHeader, Now);
                }
            }

            var identity = new ClaimsIdentity([new Claim(scenario == "name_identifier" ? ClaimTypes.NameIdentifier : "sub", (string)Reporters.Subject)], "Specifications");
            if (scenario != "no_actor")
            {
                identity.Actor = new ClaimsIdentity([new Claim(scenario == "name_identifier" ? ClaimTypes.NameIdentifier : "sub", "support-person")], "Specifications");
            }

            _authentication.AuthenticateAsync(Arg.Any<HttpContext>(), Arg.Any<string?>()).Returns(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), "Specifications")));
            var arc = AuthProxyTokens.ArcOptions(_authProxy);
            arc.CurrentValue.OnBehalfOf.Enabled = scenario != "disabled";
            _passedOn = false;
            var forwards = new OnBehalfForwards(
                _ =>
                {
                    _passedOn = true;
                    return Task.CompletedTask;
                },
                arc,
                new RecordingLogger<OnBehalfForwards>());
            await forwards.InvokeAsync(_http, _stores, _sessions);
            _outcomes[scenario] = (_passedOn, _http.Response.StatusCode, _http.Response.Headers.CacheControl.ToString(), Forward);
        }
    }

    void Bypassed(string scenario)
    {
        _outcomes[scenario].Passed.ShouldBeTrue();
        _outcomes[scenario].Forward.ShouldBeNull();
    }

    void Refused(string scenario)
    {
        _outcomes[scenario].Passed.ShouldBeFalse();
        _outcomes[scenario].Status.ShouldEqual(StatusCodes.Status401Unauthorized);
        _outcomes[scenario].Cache.ShouldEqual("no-store");
        _outcomes[scenario].Forward.ShouldBeNull();
    }

    [Fact] void should_bypass_when_disabled() => Bypassed("disabled");
    [Fact] void should_bypass_ordinary_requests() => Bypassed("ordinary");
    [Fact] void should_bypass_session_callbacks() => Bypassed("callback");
    [Fact] void should_require_an_authenticated_actor() => Refused("no_actor");
    [Fact] void should_require_an_assertion_even_with_an_actor() => Refused("missing_assertion");
    [Fact] void should_refuse_unreadable_assertions() => Refused("invalid_assertion");
    [Fact] void should_bind_the_represented_subject() => Refused("wrong_subject");
    [Fact] void should_mark_an_invalid_issue_not_reported() => _outcomes["invalid_issue"].Forward!.Refusal.ShouldEqual(SupportSessionRefusal.IssueNotReported);
    [Fact] void should_mark_an_unknown_tenant_not_reported() => _outcomes["unknown_tenant"].Forward!.Refusal.ShouldEqual(SupportSessionRefusal.IssueNotReported);
    [Fact] void should_accept_name_identifier_claims() => _outcomes["name_identifier"].Passed.ShouldBeTrue();
}
#endif
