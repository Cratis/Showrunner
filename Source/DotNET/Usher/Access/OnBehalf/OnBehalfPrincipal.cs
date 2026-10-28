// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Claims;
using Cratis.Arc.Identity;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// Builds the principal a session notification is recorded under: the represented user, with the actor as its
/// <see cref="ClaimsIdentity.Actor"/>, exactly as Arc shapes a forwarded on-behalf request. Arc's Chronicle integration
/// turns it into the causation identity, the actor first and the user second, so the start and end of a session carry
/// both identities like everything done in it.
/// </summary>
public static class OnBehalfPrincipal
{
    /// <summary>
    /// The authentication type of the identities it builds.
    /// </summary>
    public const string AuthenticationType = "Cratis.Showrunner.Usher.OnBehalf";

    /// <summary>
    /// Builds the principal for a verified session.
    /// </summary>
    /// <param name="assertion">The <see cref="OnBehalfAssertion"/> of the verified token.</param>
    /// <param name="actorName">The actor's name or email, as AuthProxy describes them.</param>
    /// <returns>The <see cref="ClaimsPrincipal"/>.</returns>
    public static ClaimsPrincipal For(OnBehalfAssertion assertion, string actorName)
    {
        var represented = new ClaimsIdentity(
            [
                new Claim("sub", assertion.Subject),
                new Claim(ClaimTypes.NameIdentifier, assertion.Subject)
            ],
            AuthenticationType,
            "name",
            ClaimTypes.Role)
        {
            Actor = new ClaimsIdentity(
                [
                    new Claim("sub", assertion.Actor),
                    new Claim(ClaimTypes.NameIdentifier, assertion.Actor),
                    new Claim("name", string.IsNullOrWhiteSpace(actorName) ? assertion.Actor : actorName),
                    new Claim(MicrosoftIdentityPlatformClaims.IdentityProvider, assertion.ActorIdentityProvider)
                ],
                AuthenticationType,
                "name",
                ClaimTypes.Role)
        };

        return new ClaimsPrincipal(represented);
    }
}
