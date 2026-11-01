// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Cratis.Showrunner.StageOperator.Resources;

/// <summary>Represents the requests and limits of the Stage container.</summary>
public record StageApplicationResources
{
    /// <summary>Gets the requests.</summary>
    [JsonPropertyName("requests")]
    public StageApplicationResourceList? Requests { get; init; }

    /// <summary>Gets the limits.</summary>
    [JsonPropertyName("limits")]
    public StageApplicationResourceList? Limits { get; init; }
}
