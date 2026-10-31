// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Claims;
using Cratis.Arc.Identity;

namespace Cratis.Showrunner.BoxOffice.Seats;

/// <summary>
/// Reads who the signed-in user is from the claims of the current principal: the identity provider and the subject.
/// </summary>
/// <param name="principals">The <see cref="ICurrentPrincipalAccessor"/>.</param>
public class CurrentSeatHolder(ICurrentPrincipalAccessor principals) : ICurrentSeatHolder
{
    /// <inheritdoc/>
    public SeatHolder? Current
    {
        get
        {
            var principal = principals.Current;
            if (principal?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var subject = FirstOf(principal, "sub", ClaimTypes.NameIdentifier);
            return string.IsNullOrWhiteSpace(subject)
                ? null
                : new(FirstOf(principal, MicrosoftIdentityPlatformClaims.IdentityProvider), subject);
        }
    }

    static string FirstOf(ClaimsPrincipal principal, params string[] claimTypes)
    {
        foreach (var claimType in claimTypes)
        {
            var value = principal.FindFirst(claimType)?.Value;
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return string.Empty;
    }
}
