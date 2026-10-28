// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Arc;
using Cratis.Chronicle;
using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// Answers AuthProxy's calls for on-behalf sessions: the grant check, and the notifications that a session started and
/// ended (Cratis/AuthProxy#217).
/// </summary>
/// <remarks>
/// Every call is authenticated by AuthProxy's signed token alone; who acts, for whom, in which tenant, for which issue
/// and in which session come from the token, never from the query or the body. Anything Usher cannot confirm is an
/// answer that keeps support out: an unknown tenant or issue is not live, and a start that cannot be recorded is not
/// acknowledged, so AuthProxy never issues the session.
/// </remarks>
/// <param name="arc">The <see cref="ArcOptions"/>, whose <see cref="ArcOptions.OnBehalfOf"/> turns on-behalf sessions on.</param>
/// <param name="options">The <see cref="UsherOptions"/>.</param>
/// <param name="tokens">The <see cref="OnBehalfTokens"/>.</param>
/// <param name="stores">The <see cref="ITenantEventStores"/>.</param>
/// <param name="sessions">The <see cref="SupportSessions"/>.</param>
/// <param name="principals">The <see cref="IRepresentedPrincipals"/>.</param>
/// <param name="logger">The logger.</param>
public class OnBehalfSessionCalls(
    IOptionsMonitor<ArcOptions> arc,
    IOptionsMonitor<UsherOptions> options,
    OnBehalfTokens tokens,
    ITenantEventStores stores,
    SupportSessions sessions,
    IRepresentedPrincipals principals,
    ILogger<OnBehalfSessionCalls> logger)
{
    /// <summary>
    /// The largest body Usher reads from AuthProxy.
    /// </summary>
    public const int MaximumBodyLength = 64 * 1024;

    static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Answers the grant check: whether support may act as the user right now, and who the user is.
    /// </summary>
    /// <param name="http">The <see cref="HttpContext"/> of the call.</param>
    /// <returns>The <see cref="IResult"/>.</returns>
    public async Task<IResult> CheckGrant(HttpContext http)
    {
        var (assertion, failure) = await Verify(http, OnBehalfPaths.GrantCheckPurpose, ReadOnlyMemory<byte>.Empty);
        if (assertion is null)
        {
            return failure!;
        }

        if (await RequestFor(assertion, http.RequestAborted) is not { } found)
        {
            return Results.Json(OnBehalfGrantAnswer.NotLive, _serializerOptions);
        }

        var checkedGrant = await sessions.Check(found.EventStore, found.Request);
        if (!checkedGrant.TryGetResult(out var grant))
        {
            return Results.Json(OnBehalfGrantAnswer.NotLive, _serializerOptions);
        }

        var principal = await principals.For(grant.Reporter, assertion.Tenant);
        if (!string.Equals(principal.UserId, assertion.Subject.Value, StringComparison.Ordinal) ||
            !string.Equals(principal.IdentityProvider, grant.Reporter.IdentityProvider.Value, StringComparison.Ordinal))
        {
            logger.PrincipalForSomebodyElse(found.Request.IssueId);
            return Results.Json(OnBehalfGrantAnswer.NotLive, _serializerOptions);
        }

        return Results.Json(new OnBehalfGrantAnswer(true, grant.ValidUntil.Value, principal), _serializerOptions);
    }

    /// <summary>
    /// Records that a session started, when the grant still lets it; any other answer than a success keeps AuthProxy
    /// from issuing the session.
    /// </summary>
    /// <param name="http">The <see cref="HttpContext"/> of the call.</param>
    /// <returns>The <see cref="IResult"/>.</returns>
    public async Task<IResult> SessionStarted(HttpContext http)
    {
        var (assertion, notification, failure) = await Notification(http, OnBehalfPaths.SessionStartedPurpose);
        if (assertion is null || notification is null)
        {
            return failure!;
        }

        if (await RequestFor(assertion, http.RequestAborted) is not { } found)
        {
            return Refused(SupportSessionRefusal.IssueNotReported);
        }

        http.User = OnBehalfPrincipal.For(assertion, notification.Actor.UserDetails);
        var started = await sessions.Start(found.EventStore, found.Request, AuthorOf(assertion, notification.Actor));
        return started.TryGetError(out var refusal) ? Refused(refusal) : Results.NoContent();
    }

    /// <summary>
    /// Records that a session ended, once, however many times AuthProxy reports it.
    /// </summary>
    /// <param name="http">The <see cref="HttpContext"/> of the call.</param>
    /// <returns>The <see cref="IResult"/>.</returns>
    public async Task<IResult> SessionEnded(HttpContext http)
    {
        var (assertion, notification, failure) = await Notification(http, OnBehalfPaths.SessionEndedPurpose);
        if (assertion is null || notification is null)
        {
            return failure!;
        }

        if (await RequestFor(assertion, http.RequestAborted) is { } found)
        {
            http.User = OnBehalfPrincipal.For(assertion, notification.Actor.UserDetails);
            await sessions.End(found.EventStore, found.Request.IssueId, found.Request.SessionId);
        }

        return Results.NoContent();
    }

    static IResult Refused(SupportSessionRefusal refusal) =>
        Results.Json(new { error = refusal.ToString() }, _serializerOptions, statusCode: StatusCodes.Status403Forbidden);

    static async Task<byte[]?> ReadBody(HttpRequest request)
    {
        if (request.ContentLength > MaximumBodyLength)
        {
            return null;
        }

        await using var body = new MemoryStream();
        var buffer = new byte[8192];
        int read;
        while ((read = await request.Body.ReadAsync(buffer, request.HttpContext.RequestAborted)) > 0)
        {
            await body.WriteAsync(buffer.AsMemory(0, read), request.HttpContext.RequestAborted);
            if (body.Length > MaximumBodyLength)
            {
                return null;
            }
        }

        return body.ToArray();
    }

    SupportAuthor AuthorOf(OnBehalfAssertion assertion, OnBehalfActor actor)
    {
        var isAgent = options.CurrentValue.OnBehalf.AgentIdentityProviders.Contains(assertion.ActorIdentityProvider.Value, StringComparer.OrdinalIgnoreCase);
        var name = string.IsNullOrWhiteSpace(actor.UserDetails) ? assertion.Actor.Value : actor.UserDetails;
        return new(name, isAgent ? SupportAuthorKind.Agent : SupportAuthorKind.Person);
    }

    async Task<SessionInTenant?> RequestFor(OnBehalfAssertion assertion, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(assertion.Issue, out var issue) || await stores.For(assertion.Tenant, cancellationToken) is not { } eventStore)
        {
            logger.UnknownTenantOrIssue();
            return null;
        }

        return new(eventStore, new SupportSessionRequest(issue, assertion.SessionId, assertion.Subject, new(assertion.ActorIdentityProvider, assertion.Actor)));
    }

    async Task<(OnBehalfAssertion? Assertion, OnBehalfSessionNotification? Notification, IResult? Failure)> Notification(HttpContext http, string purpose)
    {
        http.Response.Headers.CacheControl = "no-store";
        if (await ReadBody(http.Request) is not { } body)
        {
            return (null, null, Results.StatusCode(StatusCodes.Status413PayloadTooLarge));
        }

        var (assertion, failure) = await Verify(http, purpose, body);
        if (assertion is null)
        {
            return (null, null, failure);
        }

        OnBehalfSessionNotification? notification;
        try
        {
            notification = JsonSerializer.Deserialize<OnBehalfSessionNotification>(body, _serializerOptions);
        }
        catch (JsonException)
        {
            // An unreadable notification is a bad request; the token already proved it came from AuthProxy.
            notification = null;
        }

        if (notification?.Actor is null || !notification.IsAbout(assertion))
        {
            logger.NotificationNotAboutTheSession(purpose);
            return (null, null, Results.BadRequest());
        }

        return (assertion, notification, null);
    }

    async Task<(OnBehalfAssertion? Assertion, IResult? Failure)> Verify(HttpContext http, string purpose, ReadOnlyMemory<byte> body)
    {
        http.Response.Headers.CacheControl = "no-store";
        if (arc.CurrentValue.OnBehalfOf?.Enabled != true)
        {
            return (null, Results.NotFound());
        }

        var call = new OnBehalfCall(purpose, http.Request.Method, http.Request.PathBase, http.Request.Path, body);
        var verified = await tokens.Verify(http.Request.Headers.Authorization, call);
        if (verified.TryGetError(out var rejection) || !verified.TryGetResult(out var assertion))
        {
            logger.TokenRejected(purpose, rejection);
            return (null, Results.Unauthorized());
        }

        var tenantHeader = http.Request.Headers[OnBehalfPaths.TenantHeader].ToString();
        if (tenantHeader.Length > 0 && !string.Equals(tenantHeader, assertion.Tenant.Value, StringComparison.Ordinal))
        {
            logger.TenantHeaderDisagrees(purpose);
            return (null, Results.BadRequest());
        }

        return (assertion, null);
    }

    sealed record SessionInTenant(IEventStore EventStore, SupportSessionRequest Request);
}
