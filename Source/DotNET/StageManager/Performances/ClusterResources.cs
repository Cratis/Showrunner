// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using Cratis.Showrunner.StageManager.Productions;

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// <see cref="IClusterResources"/> over the Kubernetes API.
/// </summary>
/// <param name="client">The Kubernetes client.</param>
public class ClusterResources(IKubernetesClient client) : IClusterResources
{
    /// <inheritdoc/>
    public async Task<string?> GetConfigMapValueAsync(string @namespace, string name, string key, CancellationToken cancellationToken)
    {
        var configMap = await client.GetAsync<V1ConfigMap>(name, @namespace, cancellationToken);
        return configMap?.Data is { } data && data.TryGetValue(key, out var value) ? value : null;
    }

    /// <inheritdoc/>
    public async Task<string?> GetSecretValueAsync(string @namespace, string name, string key, CancellationToken cancellationToken)
    {
        var secret = await client.GetAsync<V1Secret>(name, @namespace, cancellationToken);
        return secret?.Data is { } data && data.TryGetValue(key, out var value) ? Encoding.UTF8.GetString(value) : null;
    }

    /// <inheritdoc/>
    public async Task<string?> GetProductionConnectionSecretNameAsync(string @namespace, string name, CancellationToken cancellationToken)
    {
        var production = await client.GetAsync<Production>(name, @namespace, cancellationToken);
        var secret = production?.Status?.ConnectionSecretName;
        return string.IsNullOrEmpty(secret) ? null : secret;
    }

    /// <inheritdoc/>
    public async Task ApplyAsync<TResource>(TResource desired, CancellationToken cancellationToken)
        where TResource : IKubernetesObject<V1ObjectMeta>
    {
        var existing = await client.GetAsync<TResource>(desired.Metadata.Name, desired.Metadata.NamespaceProperty, cancellationToken);
        if (existing is null)
        {
            await client.CreateAsync(desired, cancellationToken);
            return;
        }

        desired.Metadata.ResourceVersion = existing.Metadata.ResourceVersion;
        if (desired is V1Service service && existing is V1Service current)
        {
            // The cluster IP is assigned once and cannot be changed.
            service.Spec.ClusterIP = current.Spec.ClusterIP;
            service.Spec.ClusterIPs = current.Spec.ClusterIPs;
        }

        await client.UpdateAsync(desired, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DeploymentReadiness?> GetDeploymentReadinessAsync(string @namespace, string name, CancellationToken cancellationToken)
    {
        var deployment = await client.GetAsync<V1Deployment>(name, @namespace, cancellationToken);
        return deployment is null
            ? null
            : new(deployment.Spec.Replicas ?? 1, deployment.Status?.ReadyReplicas ?? 0, deployment.Status?.UpdatedReplicas ?? 0);
    }

    /// <inheritdoc/>
    public async Task UpdateStatusAsync(Performance performance, CancellationToken cancellationToken) =>
        await client.UpdateStatusAsync(performance, cancellationToken);
}
