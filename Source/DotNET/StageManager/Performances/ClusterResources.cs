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
    public async Task<ProductionSpec?> GetProductionSpecAsync(string @namespace, string name, CancellationToken cancellationToken) =>
        (await client.GetAsync<Production>(name, @namespace, cancellationToken))?.Spec;

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
            : new(
                deployment.Spec.Replicas ?? 1,
                deployment.Status?.ReadyReplicas ?? 0,
                deployment.Status?.UpdatedReplicas ?? 0,
                deployment.Status?.Replicas ?? 0,
                (deployment.Status?.ObservedGeneration ?? 0) >= (deployment.Metadata.Generation ?? 0));
    }

    /// <inheritdoc/>
    public async Task UpdateStatusAsync(Performance performance, CancellationToken cancellationToken) =>
        await client.UpdateStatusAsync(performance, cancellationToken);

    /// <inheritdoc/>
    public async Task<PackageState?> GetPackageStateAsync(string @namespace, string name, string key, CancellationToken cancellationToken)
    {
        var configMap = await client.GetAsync<V1ConfigMap>(name, @namespace, cancellationToken);
        if (configMap is null)
        {
            return null;
        }

        var hasKey = (configMap.BinaryData?.ContainsKey(key) ?? false) || (configMap.Data?.ContainsKey(key) ?? false);
        return new(hasKey, configMap.Immutable == true);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync<TResource>(string @namespace, string name, CancellationToken cancellationToken)
        where TResource : IKubernetesObject<V1ObjectMeta>
    {
        var existing = await client.GetAsync<TResource>(name, @namespace, cancellationToken);
        if (existing is not null)
        {
            await client.DeleteAsync(existing, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task ScaleDeploymentAsync(string @namespace, string name, int replicas, CancellationToken cancellationToken)
    {
        var deployment = await client.GetAsync<V1Deployment>(name, @namespace, cancellationToken);
        if (deployment is null || deployment.Spec.Replicas == replicas)
        {
            return;
        }

        deployment.Spec.Replicas = replicas;
        await client.UpdateAsync(deployment, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<UsageSample?> GetUsageAsync(string @namespace, string performance, CancellationToken cancellationToken)
    {
        IList<PodMetrics> pods;
        try
        {
            pods = await client.ListAsync<PodMetrics>(@namespace, $"{PerformanceMetadata.PerformanceLabel}={performance}", cancellationToken: cancellationToken);
        }
        catch (k8s.Autorest.HttpOperationException)
        {
            // The metrics API is an add-on; a cluster without it has no usage to report.
            return null;
        }

        if (pods.Count == 0)
        {
            return null;
        }

        var containers = pods.SelectMany(pod => pod.Containers).ToList();
        var cpu = containers.Sum(container => container.Usage.TryGetValue("cpu", out var quantity) ? (double)quantity.ToDecimal() : 0);
        var memory = containers.Sum(container => container.Usage.TryGetValue("memory", out var quantity) ? (double)quantity.ToDecimal() : 0);
        return new(cpu, memory);
    }
}
