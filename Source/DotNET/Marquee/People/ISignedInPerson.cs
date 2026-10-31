// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Claims;

namespace Cratis.Showrunner.Marquee.People;

/// <summary>
/// Defines the person signed in to the product, read from the current principal.
/// </summary>
public interface ISignedInPerson
{
    /// <summary>
    /// Gets a value indicating whether a person is signed in: the principal is authenticated and has a subject.
    /// </summary>
    bool IsSignedIn { get; }

    /// <summary>
    /// Gets the person's subject at their identity provider, <see cref="UserSubject.NotSet"/> when no one is signed in.
    /// </summary>
    UserSubject Subject { get; }
}

/// <summary>
/// Represents an implementation of <see cref="ISignedInPerson"/> that reads the claims of the current principal.
/// </summary>
/// <param name="principals">The <see cref="ICurrentPrincipalAccessor"/> giving the current principal.</param>
public class SignedInPerson(ICurrentPrincipalAccessor principals) : ISignedInPerson
{
    /// <inheritdoc/>
    public bool IsSignedIn => principals.Current?.Identity?.IsAuthenticated == true && Subject != UserSubject.NotSet;

    /// <inheritdoc/>
    public UserSubject Subject
    {
        get
        {
            foreach (var claimType in new[] { "sub", ClaimTypes.NameIdentifier })
            {
                var value = principals.Current?.FindFirst(claimType)?.Value;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return UserSubject.NotSet;
        }
    }
}
