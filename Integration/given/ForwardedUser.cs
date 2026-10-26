// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Cratis.Showrunner.Usher.Integration.given;

/// <summary>
/// A user in the client principal format AuthProxy forwards.
/// </summary>
/// <param name="IdentityProvider">The identity provider.</param>
/// <param name="UserId">The subject.</param>
/// <param name="UserDetails">The name or email.</param>
/// <param name="UserRoles">The roles.</param>
/// <param name="Claims">The claims.</param>
public record ForwardedUser(string IdentityProvider, string UserId, string UserDetails, IEnumerable<string> UserRoles, IEnumerable<ForwardedUserClaim> Claims)
{
    /// <summary>
    /// Gets the actor as the session notifications describe them.
    /// </summary>
    [JsonIgnore]
    public object Notification => new { identityProvider = IdentityProvider, userId = UserId, userDetails = UserDetails };
}
