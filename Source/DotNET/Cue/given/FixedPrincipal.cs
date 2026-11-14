// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Identity;

namespace Cratis.Showrunner.Cue.given;

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
