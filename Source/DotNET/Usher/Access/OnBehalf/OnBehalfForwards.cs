// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Claims;
using Cratis.Arc;
using Cratis.Arc.Identity;
using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.Tenancy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// Refuses a request forwarded as somebody else unless Arc accepted it as an on-behalf forward, and decides from the
/// issue's own events whether the session it was forwarded in is live.
/// </summary>
/// <remarks>
/// <para>
/// Arc verifies AuthProxy's <c>x-cratis-on-behalf-assertion</c> itself (<c>Cratis:Arc:OnBehalfOf</c>): the signature,
/// issuer, audience, lifetime, purpose, service, method, path, the exact identity headers, the tenant and that it is used
/// once. Usher does not verify it again. It asks Arc's authentication for the request, and a request that carries an
/// actor or an assertion is refused with <c>401</c> unless Arc accepted it with the actor beside the represented user,
/// so a rejected forward never reaches the application as anybody, not even anonymously.
/// </para>
/// <para>
/// Arc does not tell which issue and which session the request belongs to, so Usher reads those from the claims of the
/// assertion Arc accepted. It reads them only from the exact header value Arc authenticated, after checking that the
/// assertion names the represented user, the actor and the tenant of the request. It then decides from the issue's own
/// events whether the session is live right now - started and not ended, the grant in effect and not past its end, the
/// issue not withdrawn - and keeps the decision with the request as a <see cref="VerifiedForward"/>. Usher's commands
/// and queries let support act only in a live session, so a revocation counts the moment it is recorded, not at
/// AuthProxy's next grant check. The request itself is let through: the rest of the application is AuthProxy's to stop.
/// </para>
/// </remarks>
/// <param name="next">The next middleware.</param>
/// <param name="arc">The <see cref="ArcOptions"/>.</param>
/// <param name="logger">The logger.</param>
public class OnBehalfForwards(RequestDelegate next, IOptionsMonitor<ArcOptions> arc, ILogger<OnBehalfForwards> logger)
{
    /// <summary>
    /// The key of the <see cref="OnBehalf.VerifiedForward"/> in <see cref="HttpContext.Items"/>.
    /// </summary>
    public static readonly object VerifiedForward = new();

    /// <summary>
    /// Handles a request.
    /// </summary>
    /// <param name="http">The <see cref="HttpContext"/>.</param>
    /// <param name="stores">The <see cref="ITenantEventStores"/> giving the event store of the session's tenant.</param>
    /// <param name="sessions">The <see cref="SupportSessions"/> deciding whether the session is live.</param>
    /// <returns>An awaitable task.</returns>
    public async Task InvokeAsync(HttpContext http, ITenantEventStores stores, SupportSessions sessions)
    {
        var headers = http.Request.Headers;
        var actsOnBehalf = headers.ContainsKey(OnBehalfPaths.ActorHeader) || headers.ContainsKey(OnBehalfPaths.AssertionHeader);
        if (arc.CurrentValue.OnBehalfOf?.Enabled != true || !actsOnBehalf || http.Request.Path.StartsWithSegments(OnBehalfPaths.Prefix, StringComparison.Ordinal))
        {
            await next(http);
            return;
        }

        // Arc's authentication handler runs once per request: the pipeline's own authentication later gets this result.
        var authenticated = await http.AuthenticateAsync();
        var principal = authenticated.Succeeded ? authenticated.Principal : null;
        if (principal?.GetActor() is not { } actor)
        {
            logger.ForwardNotAccepted();
            await Refuse(http);
            return;
        }

        var assertion = OnBehalfAssertionClaims.FromVerifiedForward(headers[OnBehalfPaths.AssertionHeader].ToString());
        if (assertion is null ||
            !string.Equals(assertion.Subject.Value, SubjectOf(principal.Identities.First(_ => _.Actor is not null)), StringComparison.Ordinal) ||
            !string.Equals(assertion.Actor.Value, SubjectOf(actor), StringComparison.Ordinal))
        {
            logger.ForwardWithoutSession();
            await Refuse(http);
            return;
        }

        if (!string.Equals(headers[OnBehalfPaths.TenantHeader].ToString(), assertion.Tenant.Value, StringComparison.Ordinal))
        {
            logger.ForwardInAnotherTenant();
            await Refuse(http);
            return;
        }

        http.Items[VerifiedForward] = new VerifiedForward(assertion, await RefusalFor(assertion, stores, sessions, http.RequestAborted));
        await next(http);
    }

    static string? SubjectOf(ClaimsIdentity identity) =>
        identity.FindFirst("sub")?.Value ?? identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    static async Task<SupportSessionRefusal?> RefusalFor(OnBehalfAssertion assertion, ITenantEventStores stores, SupportSessions sessions, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(assertion.Issue, out var issue) || await stores.For(assertion.Tenant, cancellationToken) is not { } eventStore)
        {
            return SupportSessionRefusal.IssueNotReported;
        }

        var request = new SupportSessionRequest(issue, assertion.SessionId, assertion.Subject, new(assertion.ActorIdentityProvider, assertion.Actor));
        var checkedSession = await sessions.CheckSession(eventStore, request);
        return checkedSession.TryGetError(out var refusal) ? refusal : null;
    }

    static Task Refuse(HttpContext http)
    {
        http.Response.StatusCode = StatusCodes.Status401Unauthorized;
        http.Response.Headers.CacheControl = "no-store";
        return Task.CompletedTask;
    }
}
