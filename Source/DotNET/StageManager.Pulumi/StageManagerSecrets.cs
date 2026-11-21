// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;
using Pulumi.Kubernetes.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Meta.V1;
using KubernetesProvider = Pulumi.Kubernetes.Provider;

namespace Cratis.Showrunner.StageManager.Pulumi;

/// <summary>
/// Writes the Kubernetes Secrets a Stage Manager resource names, so a credential is a Pulumi secret and never appears in a manifest.
/// </summary>
internal static class StageManagerSecrets
{
    /// <summary>
    /// Marks a credential as a Pulumi secret, so it is encrypted in the state and masked in the output.
    /// </summary>
    /// <param name="value">The credential, which may be a plain value or the output of another resource.</param>
    /// <returns>The credential as a Pulumi secret.</returns>
    public static Output<string> Protect(Input<string> value) => Output.CreateSecret(value.ToOutput());

    /// <summary>
    /// Creates a Secret next to the resource that names it.
    /// </summary>
    /// <param name="parent">The component that owns the Secret.</param>
    /// <param name="provider">The Kubernetes provider, or null for the default one.</param>
    /// <param name="pulumiName">The Pulumi name of the Secret.</param>
    /// <param name="secretName">The Kubernetes name of the Secret.</param>
    /// <param name="namespace">The namespace of the Secret.</param>
    /// <param name="labels">The labels to put on the Secret.</param>
    /// <param name="data">The values by key. Each becomes a Pulumi secret.</param>
    /// <param name="dependsOn">The resources the Secret waits for, such as the namespace.</param>
    /// <returns>The Secret.</returns>
    public static Secret Create(
        Resource parent,
        KubernetesProvider? provider,
        string pulumiName,
        string secretName,
        Input<string> @namespace,
        IReadOnlyDictionary<string, string> labels,
        IEnumerable<KeyValuePair<string, Input<string>>> data,
        IEnumerable<Resource> dependsOn)
    {
        var stringData = new InputMap<string>();
        foreach (var (key, value) in data)
        {
            stringData[key] = Protect(value);
        }

        var metadata = new ObjectMetaArgs { Name = secretName, Namespace = @namespace };
        foreach (var (label, labelValue) in labels)
        {
            metadata.Labels.Add(label, labelValue);
        }

        return new Secret(
            pulumiName,
            new SecretArgs { Metadata = metadata, StringData = stringData },
            new CustomResourceOptions { Parent = parent, Provider = provider, DependsOn = [.. dependsOn] });
    }
}
