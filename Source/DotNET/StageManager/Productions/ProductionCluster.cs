// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Performances;

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// <see cref="IProductionCluster"/> over the Kubernetes API.
/// </summary>
/// <param name="client">The Kubernetes client.</param>
/// <param name="resources">The cluster resources shared with the Performance reconciliation, for reading Secrets and applying resources the same way.</param>
public class ProductionCluster(IKubernetesClient client, IClusterResources resources) : IProductionCluster
{
    /// <inheritdoc/>
    public Task<string?> GetSecretValueAsync(string @namespace, string name, string key, CancellationToken cancellationToken) =>
        resources.GetSecretValueAsync(@namespace, name, key, cancellationToken);

    /// <inheritdoc/>
    public async Task<bool> HasDefinitionAsync(string name, CancellationToken cancellationToken) =>
        await client.GetAsync<V1CustomResourceDefinition>(name, cancellationToken: cancellationToken) is not null;

    /// <inheritdoc/>
    public Task ApplyAsync<TResource>(TResource desired, CancellationToken cancellationToken)
        where TResource : IKubernetesObject<V1ObjectMeta> =>
        resources.ApplyAsync(desired, cancellationToken);

    /// <inheritdoc/>
    public async Task<string?> GetMongoDBStateAsync(string @namespace, string name, CancellationToken cancellationToken) =>
        ManagedMongoDB.StateOf(await client.GetAsync<PerconaServerMongoDB>(name, @namespace, cancellationToken));

    /// <inheritdoc/>
    public async Task<DeploymentReadiness?> GetStatefulSetReadinessAsync(string @namespace, string name, CancellationToken cancellationToken)
    {
        var statefulSet = await client.GetAsync<V1StatefulSet>(name, @namespace, cancellationToken);
        return statefulSet is null
            ? null
            : new(
                statefulSet.Spec.Replicas ?? 1,
                statefulSet.Status?.ReadyReplicas ?? 0,
                statefulSet.Status?.UpdatedReplicas ?? 0,
                statefulSet.Status?.Replicas ?? 0,
                (statefulSet.Status?.ObservedGeneration ?? 0) >= (statefulSet.Metadata.Generation ?? 0));
    }

    /// <inheritdoc/>
    public async Task UpdateStatusAsync(Production production, CancellationToken cancellationToken) =>
        await client.UpdateStatusAsync(production, cancellationToken);
}
