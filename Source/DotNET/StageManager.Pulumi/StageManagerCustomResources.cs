// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;
using Pulumi.Kubernetes.Types.Inputs.Meta.V1;
using KubernetesProvider = Pulumi.Kubernetes.Provider;

namespace Cratis.Showrunner.StageManager.Pulumi;

/// <summary>
/// Declares the custom resources of the <c>showrunner.cratis.io</c> group.
/// </summary>
internal static class StageManagerCustomResources
{
    /// <summary>
    /// Declares a custom resource.
    /// </summary>
    /// <param name="parent">The component that owns the resource.</param>
    /// <param name="provider">The Kubernetes provider, or null for the default one.</param>
    /// <param name="pulumiName">The Pulumi name of the resource.</param>
    /// <param name="kind">The kind: <c>Production</c>, <c>Performance</c> or <c>Understudy</c>.</param>
    /// <param name="name">The Kubernetes name.</param>
    /// <param name="namespace">The namespace.</param>
    /// <param name="labels">The labels.</param>
    /// <param name="spec">The spec as plain values.</param>
    /// <param name="dependsOn">The resources it waits for, such as the Secrets it names.</param>
    /// <returns>The custom resource.</returns>
    public static Resource Declare(
        Resource parent,
        KubernetesProvider? provider,
        string pulumiName,
        string kind,
        string name,
        Input<string> @namespace,
        IReadOnlyDictionary<string, string> labels,
        Dictionary<string, object> spec,
        IEnumerable<Resource> dependsOn)
    {
        var metadata = new ObjectMetaArgs { Name = name, Namespace = @namespace };
        foreach (var (label, value) in labels)
        {
            metadata.Labels.Add(label, value);
        }

        var args = new StageManagerCustomResourceArgs(kind) { Metadata = metadata, Spec = spec };
        var options = new CustomResourceOptions { Parent = parent, Provider = provider, DependsOn = [.. dependsOn] };
        return new StageManagerCustomResource(pulumiName, args, options);
    }
}
