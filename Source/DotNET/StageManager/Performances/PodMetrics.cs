// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// The metrics of one pod, as the metrics API (metrics.k8s.io) reports them. Only read; it is not a resource of Stage Manager.
/// </summary>
[KubernetesEntity(Group = "metrics.k8s.io", ApiVersion = "v1beta1", Kind = "PodMetrics", PluralName = "pods")]
public class PodMetrics : IKubernetesObject<V1ObjectMeta>
{
    /// <inheritdoc/>
    public string ApiVersion { get; set; } = "metrics.k8s.io/v1beta1";

    /// <inheritdoc/>
    public string Kind { get; set; } = "PodMetrics";

    /// <inheritdoc/>
    public V1ObjectMeta Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the usage per container.
    /// </summary>
    [JsonPropertyName("containers")]
    public IList<ContainerMetrics> Containers { get; set; } = [];
}
