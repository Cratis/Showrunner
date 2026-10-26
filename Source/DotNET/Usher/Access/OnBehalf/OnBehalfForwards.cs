// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// Refuses a request forwarded as somebody else unless AuthProxy's signed assertion vouches for it: a request that
/// carries an actor is let through only with an <c>x-cratis-on-behalf-assertion</c> that verifies and is bound to the
/// exact identity headers and the tenant of the request.
/// </summary>
/// <remarks>
/// <para>
/// Arc turns the forwarded headers into the represented user with the actor beside them, but the headers are not
/// signed. This is what keeps a caller that reached the backend some other way than through AuthProxy from forging a
/// session. It runs only when on-behalf sessions are configured, and leaves AuthProxy's own calls, which carry a token
/// of their own, to their endpoints.
/// </para>
/// <para>
/// For a forward it lets through, it also decides from the issue's own events whether the session is live right now -
/// the grant in effect and not past its end, the issue not withdrawn, the session not ended - and keeps the decision
/// with the request as a <see cref="VerifiedForward"/>. Usher's commands and queries let support act only in a live
/// session, so a revocation counts the moment it is recorded, not at AuthProxy's next grant check. The request itself
/// is let through: the rest of the application is AuthProxy's to stop.
/// </para>
/// </remarks>
/// <param name="next">The next middleware.</param>
/// <param name="options">The <see cref="UsherOptions"/>.</param>
/// <param name="logger">The logger.</param>
public class OnBehalfForwards(RequestDelegate next, IOptionsMonitor<UsherOptions> options, ILogger<OnBehalfForwards> logger)
{
    /// <summary>
    /// The key of the <see cref="OnBehalf.VerifiedForward"/> in <see cref="HttpContext.Items"/>.
    /// </summary>
    public static readonly object VerifiedForward = new();

    /// <summary>
    /// Handles a request.
    /// </summary>
    /// <param name="http">The <see cref="HttpContext"/>.</param>
    /// <param name="tokens">The <see cref="OnBehalfTokens"/>.</param>
    /// <param name="stores">The <see cref="ITenantEventStores"/> giving the event store of the session's tenant.</param>
    /// <param name="sessions">The <see cref="SupportSessions"/> deciding whether the session is live.</param>
    /// <returns>An awaitable task.</returns>
    public async Task InvokeAsync(HttpContext http, OnBehalfTokens tokens, ITenantEventStores stores, SupportSessions sessions)
    {
        var headers = http.Request.Headers;
        var actsOnBehalf = headers.ContainsKey(OnBehalfPaths.ActorHeader) || headers.ContainsKey(OnBehalfPaths.AssertionHeader);
        if (!options.CurrentValue.OnBehalf.IsEnabled || !actsOnBehalf || http.Request.Path.StartsWithSegments(OnBehalfPaths.Prefix, StringComparison.Ordinal))
        {
            await next(http);
            return;
        }

        var verified = await tokens.VerifyForward(
            headers[OnBehalfPaths.AssertionHeader].ToString(),
            headers[OnBehalfPaths.PrincipalHeader].ToString(),
            headers[OnBehalfPaths.ActorHeader].ToString());
        if (verified.TryGetError(out var rejection) || !verified.TryGetResult(out var assertion))
        {
            logger.ForwardRefused(rejection);
            await Refuse(http);
            return;
        }

        var tenant = headers[OnBehalfPaths.TenantHeader].ToString();
        if (!string.Equals(tenant, assertion.Tenant.Value, StringComparison.Ordinal))
        {
            logger.ForwardInAnotherTenant();
            await Refuse(http);
            return;
        }

        http.Items[VerifiedForward] = new VerifiedForward(assertion, await RefusalFor(assertion, stores, sessions, http.RequestAborted));
        await next(http);
    }

    static async Task<SupportSessionRefusal?> RefusalFor(OnBehalfAssertion assertion, ITenantEventStores stores, SupportSessions sessions, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(assertion.Issue, out var issue) || await stores.For(assertion.Tenant, cancellationToken) is not { } eventStore)
        {
            return SupportSessionRefusal.IssueNotReported;
        }

        var request = new SupportSessionRequest(issue, assertion.SessionId, assertion.Subject, new(assertion.ActorIdentityProvider, assertion.Actor));
        var checkedGrant = await sessions.CheckSession(eventStore, request);
        return checkedGrant.TryGetError(out var refusal) ? refusal : null;
    }

    static Task Refuse(HttpContext http)
    {
        http.Response.StatusCode = StatusCodes.Status401Unauthorized;
        http.Response.Headers.CacheControl = "no-store";
        return Task.CompletedTask;
    }
}
