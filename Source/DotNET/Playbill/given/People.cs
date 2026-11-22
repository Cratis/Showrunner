// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;

namespace Cratis.Showrunner.Playbill.given;

/// <summary>
/// The people the specifications are about.
/// </summary>
public static class People
{
    /// <summary>
    /// The person the specifications act as.
    /// </summary>
    public static readonly ViewerSubject Me = new("person-1");

    /// <summary>
    /// Another person.
    /// </summary>
    public static readonly ViewerSubject SomebodyElse = new("person-2");

    /// <summary>
    /// Gets the principal of a person who signed in.
    /// </summary>
    /// <param name="subject">The person's subject.</param>
    /// <returns>The principal.</returns>
    public static ClaimsPrincipal SignedInAs(ViewerSubject subject) => new(new ClaimsIdentity([new Claim("sub", subject.Value)], "Specifications"));

    /// <summary>
    /// Gets the principal of someone who did not sign in.
    /// </summary>
    public static ClaimsPrincipal Anonymous => new(new ClaimsIdentity());
}
#endif
