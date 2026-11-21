// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Cluster;
using k8s;
using k8s.Autorest;

namespace Cratis.Showrunner.Operations.Kubernetes;

/// <summary>
/// An <see cref="IVolumeHandles"/> that reads the cluster's PersistentVolumes with the Kubernetes client.
/// </summary>
/// <param name="client">The Kubernetes client.</param>
public sealed class KubernetesVolumeHandles(IKubernetes client) : IVolumeHandles
{
    /// <inheritdoc/>
    public async Task<IReadOnlySet<string>> Read(string driver, CancellationToken cancellationToken)
    {
        try
        {
            var volumes = await client.CoreV1.ListPersistentVolumeAsync(cancellationToken: cancellationToken);
            return volumes.Items
                .Where(volume => string.Equals(volume.Spec?.Csi?.Driver, driver, StringComparison.Ordinal))
                .Select(volume => volume.Spec.Csi.VolumeHandle)
                .ToHashSet(StringComparer.Ordinal);
        }
        catch (HttpOperationException error)
        {
            throw new ClusterReadException($"could not read persistent volumes: HTTP {(int)error.Response.StatusCode}", (int)error.Response.StatusCode, error);
        }
    }
}
