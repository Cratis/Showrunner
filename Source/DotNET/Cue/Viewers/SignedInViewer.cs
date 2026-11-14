// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Claims;

namespace Cratis.Showrunner.Cue.Viewers;

/// <summary>
/// Represents an implementation of <see cref="ISignedInViewer"/> that reads the claims of the current principal.
/// </summary>
/// <param name="principals">The <see cref="ICurrentPrincipalAccessor"/> giving the current principal.</param>
public class SignedInViewer(ICurrentPrincipalAccessor principals) : ISignedInViewer
{
    /// <inheritdoc/>
    public bool IsSignedIn => principals.Current?.Identity?.IsAuthenticated == true && Subject != ViewerSubject.NotSet;

    /// <inheritdoc/>
    public ViewerSubject Subject
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

            return ViewerSubject.NotSet;
        }
    }
}
