// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// A user in AuthProxy's client principal format: who support acts as in a session. AuthProxy forwards it as
/// <c>x-ms-client-principal</c> on every request of the session, so it carries what the application authorizes on.
/// </summary>
/// <param name="IdentityProvider">The user's identity provider.</param>
/// <param name="UserId">The user's subject; it is always the subject the grant check asked about.</param>
/// <param name="UserDetails">The user's name or email.</param>
/// <param name="UserRoles">The user's roles in the application.</param>
/// <param name="Claims">The user's claims.</param>
public record OnBehalfClientPrincipal(
    [property: JsonPropertyName("identityProvider")] string IdentityProvider,
    [property: JsonPropertyName("userId")] string UserId,
    [property: JsonPropertyName("userDetails")] string UserDetails,
    [property: JsonPropertyName("userRoles")] IEnumerable<string> UserRoles,
    [property: JsonPropertyName("claims")] IEnumerable<OnBehalfClientPrincipalClaim> Claims);
