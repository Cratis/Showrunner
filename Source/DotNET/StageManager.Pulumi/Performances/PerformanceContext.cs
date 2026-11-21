// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;
using KubernetesProvider = Pulumi.Kubernetes.Provider;

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// What the args of a <see cref="Performance"/> need to declare the Secrets, ConfigMaps and claims their settings refer to.
/// </summary>
/// <param name="Parent">The component the resources belong to.</param>
/// <param name="Provider">The Kubernetes provider, or null for the default one.</param>
/// <param name="Name">The Pulumi name of the component.</param>
/// <param name="KubernetesName">The Kubernetes name of the Performance.</param>
/// <param name="Namespace">The namespace of the Performance.</param>
/// <param name="Labels">The labels put on every resource.</param>
/// <param name="DependsOn">What the resources wait for.</param>
internal sealed record PerformanceContext(
    Resource Parent,
    KubernetesProvider? Provider,
    string Name,
    string KubernetesName,
    Input<string> Namespace,
    IReadOnlyDictionary<string, string> Labels,
    IReadOnlyList<Resource> DependsOn)
{
    /// <summary>
    /// Gets the resources declared for the settings, which the Performance waits for.
    /// </summary>
    public List<Resource> Declared { get; } = [];
}
