// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Identity;

namespace Cratis.Showrunner.Notes.Collecting.given;

/// <summary>
/// The people who act on the collecting side in the specifications.
/// </summary>
public static class Operators
{
    /// <summary>
    /// Creates a signed-in principal.
    /// </summary>
    /// <returns>The principal.</returns>
    public static ClaimsPrincipal SignedIn() => new(new ClaimsIdentity([new Claim("sub", "operator-1")], "Specifications"));

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
