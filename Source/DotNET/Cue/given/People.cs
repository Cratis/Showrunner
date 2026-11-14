// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Identity;

namespace Cratis.Showrunner.Cue.given;

/// <summary>
/// The people the specifications run as.
/// </summary>
public static class People
{
    /// <summary>
    /// The subject of the person the specifications run as.
    /// </summary>
    public static readonly ViewerSubject Me = new("person-1");

    /// <summary>
    /// The subject of somebody else.
    /// </summary>
    public static readonly ViewerSubject SomebodyElse = new("person-2");

    /// <summary>
    /// Gets a signed-in principal.
    /// </summary>
    /// <param name="subject">The subject of the person.</param>
    /// <returns>The principal.</returns>
    public static ClaimsPrincipal SignedInAs(ViewerSubject subject) => new(new ClaimsIdentity([new Claim("sub", subject.Value)], "Specifications"));

    /// <summary>
    /// Gets a principal nobody is signed in as.
    /// </summary>
    public static ClaimsPrincipal Anonymous => new(new ClaimsIdentity());
}
#endif
