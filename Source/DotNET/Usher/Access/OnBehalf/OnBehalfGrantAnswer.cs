// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// The answer to AuthProxy's grant check.
/// </summary>
/// <param name="Live">Whether support may act as the user right now.</param>
/// <param name="ValidUntil">When the grant ends; only on a live answer.</param>
/// <param name="Principal">Who support acts as; only on a live answer.</param>
public record OnBehalfGrantAnswer(
    [property: JsonPropertyName("live")] bool Live,
    [property: JsonPropertyName("validUntil"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] DateTimeOffset? ValidUntil,
    [property: JsonPropertyName("principal"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] OnBehalfClientPrincipal? Principal)
{
    /// <summary>
    /// The answer that support may not act as the user.
    /// </summary>
    public static readonly OnBehalfGrantAnswer NotLive = new(false, null, null);
}
