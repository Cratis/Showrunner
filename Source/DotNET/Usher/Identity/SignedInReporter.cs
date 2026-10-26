// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Claims;
using Cratis.Arc.Identity;
using Cratis.Showrunner.Usher.Contracts.Identity;

namespace Cratis.Showrunner.Usher.Identity;

/// <summary>
/// Defines the reporter signed in to the application, read from the current principal.
/// </summary>
public interface ISignedInReporter
{
    /// <summary>
    /// Gets a value indicating whether a reporter is signed in: the principal is authenticated and has a subject.
    /// </summary>
    bool IsSignedIn { get; }

    /// <summary>
    /// Gets the reporter's subject at their identity provider, <see cref="IdentityProviderSubject.NotSet"/> when no
    /// one is signed in. It is a pseudonymous id and is what Usher matches issues to their reporter by.
    /// </summary>
    IdentityProviderSubject Subject { get; }

    /// <summary>
    /// Gets the reporter as the desk learns them when they share who they are.
    /// </summary>
    Reporter Reporter { get; }
}

/// <summary>
/// Represents an implementation of <see cref="ISignedInReporter"/> that reads the claims of the current principal.
/// </summary>
/// <param name="principals">The <see cref="ICurrentPrincipalAccessor"/> giving the current principal.</param>
public class SignedInReporter(ICurrentPrincipalAccessor principals) : ISignedInReporter
{
    /// <inheritdoc/>
    public bool IsSignedIn => Principal?.Identity?.IsAuthenticated == true && Subject != IdentityProviderSubject.NotSet;

    /// <inheritdoc/>
    public IdentityProviderSubject Subject => FirstOf("sub", ClaimTypes.NameIdentifier);

    /// <inheritdoc/>
    public Reporter Reporter => new(
        FirstOf(MicrosoftIdentityPlatformClaims.IdentityProvider),
        Subject,
        FirstOf("name", ClaimTypes.Name),
        FirstOf("email", ClaimTypes.Email));

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
