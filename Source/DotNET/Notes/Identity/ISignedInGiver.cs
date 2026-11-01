// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Claims;

namespace Cratis.Showrunner.Notes.Identity;

/// <summary>
/// Defines the user signed in to the application, read from the current principal. It is the only place the giver of a
/// note comes from: never from what a client sends.
/// </summary>
public interface ISignedInGiver
{
    /// <summary>
    /// Gets a value indicating whether a user is signed in: the principal is authenticated and has a subject.
    /// </summary>
    bool IsSignedIn { get; }

    /// <summary>
    /// Gets the user's id at their identity provider, <see cref="GiverUserId.NotSet"/> when no one is signed in.
    /// </summary>
    GiverUserId UserId { get; }

    /// <summary>
    /// Gets the user as a note carries them.
    /// </summary>
    NoteGiver Giver { get; }
}

/// <summary>
/// Represents an implementation of <see cref="ISignedInGiver"/> that reads the claims of the current principal.
/// </summary>
/// <param name="principals">The <see cref="ICurrentPrincipalAccessor"/> giving the current principal.</param>
public class SignedInGiver(ICurrentPrincipalAccessor principals) : ISignedInGiver
{
    /// <inheritdoc/>
    public bool IsSignedIn => Principal?.Identity?.IsAuthenticated == true && UserId != GiverUserId.NotSet;

    /// <inheritdoc/>
    public GiverUserId UserId => FirstOf("sub", ClaimTypes.NameIdentifier);

    /// <inheritdoc/>
    public NoteGiver Giver => new(UserId, FirstOf("name", ClaimTypes.Name));

    ClaimsPrincipal? Principal => principals.Current;

    string FirstOf(params string[] claimTypes)
    {
        foreach (var claimType in claimTypes)
        {
            var value = Principal?.FindFirst(claimType)?.Value;
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return string.Empty;
    }
}
