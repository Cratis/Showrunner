// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Identity;
using Cratis.Showrunner.Usher.Contracts.Identity;

namespace Cratis.Showrunner.Usher.given;

/// <summary>
/// The people the specifications act as.
/// </summary>
public static class Reporters
{
    /// <summary>
    /// The subject of the reporter the specifications act as.
    /// </summary>
    public static readonly IdentityProviderSubject Subject = "8c2f6a1e-reporter";

    /// <summary>
    /// The subject of someone else.
    /// </summary>
    public static readonly IdentityProviderSubject SomeoneElse = "d41b77c0-someone-else";

    /// <summary>
    /// The reporter as they are shared with the desk.
    /// </summary>
    public static readonly Reporter Reporter = new("entra", Subject, "Ada Lovelace", "ada@example.com");

    /// <summary>
    /// Creates a signed-in principal.
    /// </summary>
    /// <param name="subject">The subject to sign in as.</param>
    /// <returns>The principal.</returns>
    public static ClaimsPrincipal SignedInAs(IdentityProviderSubject subject) => new(new ClaimsIdentity(
        [
            new Claim("sub", subject),
            new Claim("name", (string)Reporter.Name),
            new Claim("email", (string)Reporter.Email),
            new Claim(MicrosoftIdentityPlatformClaims.IdentityProvider, (string)Reporter.IdentityProvider)
        ],
        "Specifications"));

    /// <summary>
    /// Creates an anonymous principal.
    /// </summary>
    /// <returns>The principal.</returns>
    public static ClaimsPrincipal Anonymous() => new(new ClaimsIdentity());
}

/// <summary>
/// An <see cref="ICurrentPrincipalAccessor"/> that gives a fixed principal.
/// </summary>
/// <param name="principal">The principal.</param>
public class FixedPrincipal(ClaimsPrincipal principal) : ICurrentPrincipalAccessor
{
    /// <inheritdoc/>
    public ClaimsPrincipal? Current => principal;
}
#endif
