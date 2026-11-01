// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Cratis.Showrunner.StageOperator.Kubernetes;

/// <summary>The metrics of one container.</summary>
internal sealed class ContainerMetrics
{
    /// <summary>Gets or sets the container name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the usage by resource name.</summary>
    [JsonPropertyName("usage")]
    public Dictionary<string, string> Usage { get; set; } = [];
}
