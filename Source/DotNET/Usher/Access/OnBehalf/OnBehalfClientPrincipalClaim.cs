// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// A claim of an <see cref="OnBehalfClientPrincipal"/>.
/// </summary>
/// <param name="Type">The claim type.</param>
/// <param name="Value">The claim value.</param>
public record OnBehalfClientPrincipalClaim(
    [property: JsonPropertyName("typ")] string Type,
    [property: JsonPropertyName("val")] string Value);
