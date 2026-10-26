// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
/// Arc turns the forwarded headers into the represented user with the actor beside them, but the headers are not
/// signed. This is what keeps a caller that reached the backend some other way than through AuthProxy from acting as
/// a reporter. It runs only when on-behalf sessions are configured, and leaves AuthProxy's own calls, which carry a
/// token of their own, to their endpoints.
/// </remarks>
/// <param name="next">The next middleware.</param>
/// <param name="options">The <see cref="UsherOptions"/>.</param>
/// <param name="logger">The logger.</param>
public class OnBehalfForwards(RequestDelegate next, IOptionsMonitor<UsherOptions> options, ILogger<OnBehalfForwards> logger)
{
    /// <summary>
    /// The key of the verified <see cref="OnBehalfAssertion"/> in <see cref="HttpContext.Items"/>.
    /// </summary>
    public static readonly object VerifiedAssertion = new();

    /// <summary>
    /// Handles a request.
    /// </summary>
    /// <param name="http">The <see cref="HttpContext"/>.</param>
    /// <param name="tokens">The <see cref="OnBehalfTokens"/>.</param>
    /// <returns>An awaitable task.</returns>
    public async Task InvokeAsync(HttpContext http, OnBehalfTokens tokens)
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

        http.Items[VerifiedAssertion] = assertion;
        await next(http);
    }

    static Task Refuse(HttpContext http)
    {
        http.Response.StatusCode = StatusCodes.Status401Unauthorized;
        http.Response.Headers.CacheControl = "no-store";
        return Task.CompletedTask;
    }
}
