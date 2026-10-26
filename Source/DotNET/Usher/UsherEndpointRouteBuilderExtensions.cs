// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Access.OnBehalf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods for mapping Usher's own endpoints.
/// </summary>
public static class UsherEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the endpoints AuthProxy calls for on-behalf sessions: the grant check and the session notifications.
    /// Arc maps Usher's commands and queries by itself; these are plain HTTP endpoints because AuthProxy signs the exact
    /// bytes of each call. They answer <c>404</c> until <see cref="Cratis.Showrunner.Usher.UsherOptions.OnBehalf"/> is configured.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/>.</param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/> for continuation.</returns>
    /// <remarks>
    /// The endpoints allow anonymous callers: AuthProxy authenticates with its signed token, which the endpoints verify
    /// themselves, and nobody is signed in on these calls.
    /// </remarks>
    public static IEndpointRouteBuilder MapUsher(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(OnBehalfPaths.Grants, (HttpContext http, OnBehalfSessionCalls calls) => calls.CheckGrant(http))
            .AllowAnonymous()
            .ExcludeFromDescription();
        endpoints.MapPost(OnBehalfPaths.SessionStarted, (HttpContext http, OnBehalfSessionCalls calls) => calls.SessionStarted(http))
            .AllowAnonymous()
            .ExcludeFromDescription();
        endpoints.MapPost(OnBehalfPaths.SessionEnded, (HttpContext http, OnBehalfSessionCalls calls) => calls.SessionEnded(http))
            .AllowAnonymous()
            .ExcludeFromDescription();
        return endpoints;
    }
}
