// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// The support person or agent acting in a session, as AuthProxy's session notifications describe them.
/// </summary>
/// <param name="IdentityProvider">The actor's identity provider.</param>
/// <param name="UserId">The actor's subject.</param>
/// <param name="UserDetails">The actor's name or email.</param>
public record OnBehalfActor(
    [property: JsonPropertyName("identityProvider")] string IdentityProvider,
    [property: JsonPropertyName("userId")] string UserId,
    [property: JsonPropertyName("userDetails")] string UserDetails);
