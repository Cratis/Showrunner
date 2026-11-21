// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;
using Pulumi.Kubernetes.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Meta.V1;

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// Where AuthProxy keeps its data protection key ring, so sign-in sessions survive a restart.
/// </summary>
public sealed class AuthProxyDataProtectionArgs
{
    /// <summary>
    /// Gets the name of an existing PersistentVolumeClaim. Leave it out and the component creates one.
    /// </summary>
    public string? ClaimName { get; init; }

    /// <summary>
    /// Gets the size of the claim the component creates, or null for <c>1Gi</c>.
    /// </summary>
    public string? Size { get; init; }

    /// <summary>
    /// Gets the storage class of the claim the component creates, or null for the cluster's default.
    /// </summary>
    public string? StorageClassName { get; init; }

    /// <summary>
    /// Builds the spec of the key ring, creating the claim when none is named.
    /// </summary>
    /// <param name="context">The context of the Performance.</param>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec(PerformanceContext context) =>
        SpecExtensions.NewSpec().Put("claimName", ClaimName ?? DeclareClaim(context));

    string DeclareClaim(PerformanceContext context)
    {
        var name = $"{context.KubernetesName}-authproxy-keys";
        var metadata = new ObjectMetaArgs { Name = name, Namespace = context.Namespace };
        foreach (var (label, value) in context.Labels)
        {
            metadata.Labels.Add(label, value);
        }

        var spec = new PersistentVolumeClaimSpecArgs
        {
            AccessModes = ["ReadWriteOnce"],
            Resources = new VolumeResourceRequirementsArgs { Requests = { ["storage"] = Size ?? "1Gi" } }
        };
        if (StorageClassName is not null)
        {
            spec.StorageClassName = StorageClassName;
        }

        context.Declared.Add(new PersistentVolumeClaim(
            $"{context.Name}-authproxy-keys",
            new PersistentVolumeClaimArgs { Metadata = metadata, Spec = spec },
            new CustomResourceOptions { Parent = context.Parent, Provider = context.Provider, DependsOn = [.. context.DependsOn] }));
        return name;
    }
}
