// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using k8s.Models;

namespace Cratis.Showrunner.StageOperator.Kubernetes;

/// <summary>The metrics of one pod.</summary>
internal sealed class PodMetrics
{
    /// <summary>Gets or sets the pod metadata.</summary>
    [JsonPropertyName("metadata")]
    public V1ObjectMeta Metadata { get; set; } = new();

    /// <summary>Gets or sets the containers.</summary>
    [JsonPropertyName("containers")]
    public List<ContainerMetrics> Containers { get; set; } = [];
}
