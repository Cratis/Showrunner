// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Authorization;

namespace Cratis.Showrunner.Marquee.Authoring.given;

/// <summary>
/// The principals the specifications run as, and the policy a host registers to admit its operators.
/// </summary>
public static class Operators
{
    /// <summary>
    /// The role the specifications' authoring policy admits.
    /// </summary>
    public const string Role = "operator";

    /// <summary>
    /// Gets an operator of the product, signed in: somebody the host's authoring policy admits.
    /// </summary>
    public static ClaimsPrincipal Operator => new(new ClaimsIdentity([new Claim("sub", "an-operator"), new Claim(ClaimTypes.Role, Role)], "Specifications"));

    /// <summary>
    /// Gets somebody who is signed in but is not an operator, so the host's authoring policy does not admit them.
    /// </summary>
    public static ClaimsPrincipal Somebody => new(new ClaimsIdentity([new Claim("sub", "somebody")], "Specifications"));
}

/// <summary>
/// The authoring policy of the specifications' host: it admits its operators.
/// </summary>
public class AdmitOperators : IAuthorizationPolicy
{
    /// <inheritdoc/>
    public ValueTask<bool> IsAuthorized(AuthorizationPolicyContext context, CancellationToken cancellationToken) =>
        ValueTask.FromResult(context.Principal.IsInRole(Operators.Role));
}
#endif
