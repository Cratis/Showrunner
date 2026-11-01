// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Cratis.Showrunner.StageOperator.Resources;

/// <summary>Represents how long an application runs: permanently, or until a point in time.</summary>
public record StageApplicationLifetime
{
    /// <summary>Gets a value indicating whether the application runs permanently.</summary>
    [JsonPropertyName("permanent")]
    public bool? Permanent { get; init; }

    /// <summary>Gets the time the application expires, as an RFC 3339 timestamp.</summary>
    [JsonPropertyName("until")]
    public string? Until { get; init; }
}
