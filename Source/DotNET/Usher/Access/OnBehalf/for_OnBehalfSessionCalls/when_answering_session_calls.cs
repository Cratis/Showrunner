// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text;
using System.Text.Json;
using Cratis.Chronicle.Testing.Events;
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;
using Cratis.Showrunner.Usher.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfSessionCalls;

public class when_answering_session_calls : Specification
{
    static readonly DateTimeOffset _now = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);
    readonly Dictionary<string, IResult> _answers = new(StringComparer.Ordinal);
    readonly Dictionary<string, string> _cache = new(StringComparer.Ordinal);
    AuthProxyTokens _authProxy;

    void Establish() => _authProxy = new();
    void Destroy() => _authProxy.Dispose();

    async Task Because()
    {
        foreach (var operation in new[] { "grant", "start", "end" })
        {
            foreach (var scenario in new[] { "live", "disabled", "unauthorized", "tenant_header", "unknown_tenant", "invalid_issue", "missing_issue", "no_grant" })
            {
                await Call(operation, scenario);
            }
        }

        foreach (var operation in new[] { "start", "end" })
        {
            foreach (var scenario in new[] { "declared_oversize", "streamed_oversize", "malformed", "null", "missing_actor", "wrong_session", "wrong_tenant", "wrong_issue", "wrong_subject", "wrong_actor" })
            {
                await Call(operation, scenario);
            }
        }

        await Call("grant", "wrong_principal_subject");
        await Call("grant", "wrong_principal_provider");
    }

    async Task Call(string operation, string scenario)
    {
        using var eventStore = new EventStoreForTesting();
        var issueId = IssueId.New();
        if (scenario != "missing_issue")
        {
            await eventStore.EventLog.Append(issueId, IssueEvents.Described(Reporters.Identity));
            await eventStore.EventLog.Append(issueId, new ReporterSharedIdentity(Reporters.Reporter));
            if (scenario != "no_grant")
            {
                await eventStore.EventLog.Append(issueId, new ReporterGrantedAccess(_now.AddHours(1)));
            }
        }

        var facts = new OnBehalfSessionFacts((string)Reporters.SessionId, "tenant-one", scenario == "invalid_issue" ? "not-an-issue" : issueId.Value.ToString(), (string)Reporters.Subject, "support-person", "aad");
        var notification = new OnBehalfSessionNotification(facts.SessionId, facts.Tenant, facts.Issue, facts.Subject, new(facts.ActorIdentityProvider, facts.Actor, "Grace"), _now);
        notification = scenario switch
        {
            "missing_actor" => notification with { Actor = null! },
            "wrong_session" => notification with { SessionId = "another-session" },
            "wrong_tenant" => notification with { Tenant = "another-tenant" },
            "wrong_issue" => notification with { Issue = Guid.NewGuid().ToString() },
            "wrong_subject" => notification with { Subject = "another-subject" },
            "wrong_actor" => notification with { Actor = notification.Actor with { UserId = "another-actor" } },
            _ => notification
        };
        var body = operation == "grant" ? [] : JsonSerializer.SerializeToUtf8Bytes(notification);
        body = scenario switch
        {
            "malformed" => Encoding.UTF8.GetBytes("{"),
            "null" => Encoding.UTF8.GetBytes("null"),
            "declared_oversize" or "streamed_oversize" => new byte[OnBehalfSessionCalls.MaximumBodyLength + 1],
            _ => body
        };
        var path = operation switch { "start" => OnBehalfPaths.SessionStarted, "end" => OnBehalfPaths.SessionEnded, _ => OnBehalfPaths.Grants };
        var purpose = operation switch { "start" => OnBehalfPaths.SessionStartedPurpose, "end" => OnBehalfPaths.SessionEndedPurpose, _ => OnBehalfPaths.GrantCheckPurpose };
        var method = operation == "grant" ? HttpMethod.Get : HttpMethod.Post;
        var http = new DefaultHttpContext();
        http.Request.Path = path;
        http.Request.Method = method.Method;
        http.Request.Body = new MemoryStream(body);
        http.Request.ContentLength = scenario == "streamed_oversize" ? null : body.Length;
        http.Request.Headers.Authorization = scenario == "unauthorized" ? "Bearer invalid" : $"Bearer {_authProxy.ForCall(purpose, method, new Uri($"https://portal.internal{path}"), body, facts, _now)}";
        http.Request.Headers[OnBehalfPaths.TenantHeader] = scenario == "tenant_header" ? "another-tenant" : facts.Tenant;

        var arc = AuthProxyTokens.ArcOptions(_authProxy);
        if (scenario == "disabled")
        {
            arc.CurrentValue.OnBehalfOf!.Enabled = false;
        }

        var options = Substitute.For<IOptionsMonitor<UsherOptions>>();
        options.CurrentValue.Returns(new UsherOptions { ApplicationName = "Ledger", OnBehalf = new() });
        var time = new ScenarioTime(_now);
        var tokens = new OnBehalfTokens(arc, options, new OnBehalfTokenReplays(time), time);
        var stores = Substitute.For<ITenantEventStores>();
        if (scenario != "unknown_tenant")
        {
            stores.For(new TenantId(facts.Tenant), Arg.Any<CancellationToken>()).Returns(eventStore);
        }

        var principals = Substitute.For<IRepresentedPrincipals>();
        principals.For(Arg.Any<Contracts.Identity.Reporter>(), Arg.Any<TenantId>()).Returns(new OnBehalfClientPrincipal(
            scenario == "wrong_principal_provider" ? "another-provider" : (string)Reporters.IdentityProvider,
            scenario == "wrong_principal_subject" ? "another-subject" : facts.Subject,
            "Ada",
            [],
            []));
        var calls = new OnBehalfSessionCalls(arc, options, tokens, stores, new SupportSessions(time, new RecordingLogger<SupportSessions>()), principals, new RecordingLogger<OnBehalfSessionCalls>());
        var key = $"{operation}/{scenario}";
        _answers[key] = operation switch
        {
            "start" => await calls.SessionStarted(http),
            "end" => await calls.SessionEnded(http),
            _ => await calls.CheckGrant(http)
        };
        _cache[key] = http.Response.Headers.CacheControl.ToString();
    }

    int Status(string key) => ((IStatusCodeHttpResult)_answers[key]).StatusCode ?? StatusCodes.Status200OK;
    OnBehalfGrantAnswer Grant(string scenario) => (OnBehalfGrantAnswer)((IValueHttpResult)_answers[$"grant/{scenario}"]).Value!;
    void AllStatuses(string scenario, int expected)
    {
        foreach (var operation in new[] { "grant", "start", "end" })
        {
            Status($"{operation}/{scenario}").ShouldEqual(expected);
        }
    }

    void NotificationStatuses(string scenario, int expected)
    {
        Status($"start/{scenario}").ShouldEqual(expected);
        Status($"end/{scenario}").ShouldEqual(expected);
    }

    [Fact] void should_hide_disabled_endpoints() => AllStatuses("disabled", StatusCodes.Status404NotFound);
    [Fact] void should_require_authentication() => AllStatuses("unauthorized", StatusCodes.Status401Unauthorized);
    [Fact] void should_refuse_a_disagreeing_tenant_header() => AllStatuses("tenant_header", StatusCodes.Status400BadRequest);
    [Fact]
    void should_not_cache_any_answer()
    {
        foreach (var value in _cache.Values)
        {
            value.ShouldEqual("no-store");
        }
    }
    [Fact] void should_answer_a_live_grant() => Grant("live").Live.ShouldBeTrue();
    [Fact] void should_return_the_grants_end() => Grant("live").ValidUntil.ShouldEqual(_now.AddHours(1));
    [Fact] void should_refuse_an_unknown_tenant() => Grant("unknown_tenant").ShouldEqual(OnBehalfGrantAnswer.NotLive);
    [Fact] void should_refuse_an_invalid_issue() => Grant("invalid_issue").ShouldEqual(OnBehalfGrantAnswer.NotLive);
    [Fact] void should_refuse_an_unreported_issue() => Grant("missing_issue").ShouldEqual(OnBehalfGrantAnswer.NotLive);
    [Fact] void should_refuse_a_missing_grant() => Grant("no_grant").ShouldEqual(OnBehalfGrantAnswer.NotLive);
    [Fact] void should_refuse_a_principal_for_another_subject() => Grant("wrong_principal_subject").ShouldEqual(OnBehalfGrantAnswer.NotLive);
    [Fact] void should_refuse_a_principal_at_another_provider() => Grant("wrong_principal_provider").ShouldEqual(OnBehalfGrantAnswer.NotLive);
    [Fact] void should_acknowledge_a_start() => Status("start/live").ShouldEqual(StatusCodes.Status204NoContent);
    [Fact] void should_refuse_to_start_in_an_unknown_tenant() => Status("start/unknown_tenant").ShouldEqual(StatusCodes.Status403Forbidden);
    [Fact] void should_refuse_to_start_for_an_invalid_issue() => Status("start/invalid_issue").ShouldEqual(StatusCodes.Status403Forbidden);
    [Fact] void should_refuse_to_start_for_an_unreported_issue() => Status("start/missing_issue").ShouldEqual(StatusCodes.Status403Forbidden);
    [Fact] void should_refuse_to_start_without_a_grant() => Status("start/no_grant").ShouldEqual(StatusCodes.Status403Forbidden);
    [Fact] void should_acknowledge_an_end() => Status("end/live").ShouldEqual(StatusCodes.Status204NoContent);
    [Fact] void should_acknowledge_an_end_in_an_unknown_tenant() => Status("end/unknown_tenant").ShouldEqual(StatusCodes.Status204NoContent);
    [Fact] void should_acknowledge_an_end_for_an_invalid_issue() => Status("end/invalid_issue").ShouldEqual(StatusCodes.Status204NoContent);
    [Fact] void should_acknowledge_an_end_for_an_unreported_issue() => Status("end/missing_issue").ShouldEqual(StatusCodes.Status204NoContent);
    [Fact] void should_reject_an_oversized_content_length() => NotificationStatuses("declared_oversize", StatusCodes.Status413PayloadTooLarge);
    [Fact] void should_reject_an_oversized_stream() => NotificationStatuses("streamed_oversize", StatusCodes.Status413PayloadTooLarge);
    [Fact] void should_reject_malformed_json() => NotificationStatuses("malformed", StatusCodes.Status400BadRequest);
    [Fact] void should_reject_a_null_notification() => NotificationStatuses("null", StatusCodes.Status400BadRequest);
    [Fact] void should_require_an_actor() => NotificationStatuses("missing_actor", StatusCodes.Status400BadRequest);
    [Fact] void should_bind_the_session() => NotificationStatuses("wrong_session", StatusCodes.Status400BadRequest);
    [Fact] void should_bind_the_tenant() => NotificationStatuses("wrong_tenant", StatusCodes.Status400BadRequest);
    [Fact] void should_bind_the_issue() => NotificationStatuses("wrong_issue", StatusCodes.Status400BadRequest);
    [Fact] void should_bind_the_subject() => NotificationStatuses("wrong_subject", StatusCodes.Status400BadRequest);
    [Fact] void should_bind_the_actor() => NotificationStatuses("wrong_actor", StatusCodes.Status400BadRequest);
}
#endif
