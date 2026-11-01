// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Cratis.Showrunner.StageOperator.Kubernetes;

/// <summary>The <c>metrics.k8s.io/v1beta1</c> PodMetricsList, reduced to what the sampler reads.</summary>
internal sealed class PodMetricsList
{
    /// <summary>Gets or sets the pods.</summary>
    [JsonPropertyName("items")]
    public List<PodMetrics> Items { get; set; } = [];
}
