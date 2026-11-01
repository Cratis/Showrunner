// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Identity;

namespace Cratis.Showrunner.Notes.given;

/// <summary>
/// The people the specifications act as.
/// </summary>
public static class Givers
{
    /// <summary>
    /// The user id of the giver the specifications act as.
    /// </summary>
    public static readonly GiverUserId UserId = "8c2f6a1e-giver";

    /// <summary>
    /// The user id of someone else.
    /// </summary>
    public static readonly GiverUserId SomeoneElse = "d41b77c0-someone-else";

    /// <summary>
    /// The display name of the giver the specifications act as.
    /// </summary>
    public static readonly GiverDisplayName DisplayName = "Ada Lovelace";

    /// <summary>
    /// The giver as a note carries them.
    /// </summary>
    public static readonly NoteGiver Giver = new(UserId, DisplayName);

    /// <summary>
    /// Creates a signed-in principal.
    /// </summary>
    /// <param name="userId">The user id to sign in as.</param>
    /// <returns>The principal.</returns>
    public static ClaimsPrincipal SignedInAs(GiverUserId userId) => new(new ClaimsIdentity(
        [
            new Claim("sub", userId),
            new Claim("name", (string)DisplayName)
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
