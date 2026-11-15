// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using Cratis.Showrunner.StageManager.Performances;
using Cratis.Showrunner.StageManager.Productions;
using k8s.Autorest;

namespace Cratis.Showrunner.StageManager.Understudies;

/// <summary>
/// <see cref="IUnderstudyCluster"/> over the Kubernetes API.
/// </summary>
/// <param name="client">The Kubernetes client.</param>
/// <param name="resources">The cluster resources shared with the other reconciliations, for reading Secrets and applying resources the same way.</param>
public class UnderstudyCluster(IKubernetesClient client, IClusterResources resources) : IUnderstudyCluster
{
    /// <inheritdoc/>
    public Task<Production?> GetProductionAsync(string @namespace, string name, CancellationToken cancellationToken) =>
        client.GetAsync<Production>(name, @namespace, cancellationToken);

    /// <inheritdoc/>
    public Task<string?> GetSecretValueAsync(string @namespace, string name, string key, CancellationToken cancellationToken) =>
        resources.GetSecretValueAsync(@namespace, name, key, cancellationToken);

    /// <inheritdoc/>
    public Task ApplyAsync<TResource>(TResource desired, CancellationToken cancellationToken)
        where TResource : IKubernetesObject<V1ObjectMeta> =>
        resources.ApplyAsync(desired, cancellationToken);

    /// <inheritdoc/>
    public async Task<bool> TryCreateAsync<TResource>(TResource desired, CancellationToken cancellationToken)
        where TResource : IKubernetesObject<V1ObjectMeta>
    {
        try
        {
            await client.CreateAsync(desired, cancellationToken);
            return true;
        }
        catch (HttpOperationException exception) when (exception.Response.StatusCode == HttpStatusCode.Conflict)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<V1Job>> ListJobsAsync(string @namespace, string understudy, CancellationToken cancellationToken) =>
        [.. await client.ListAsync<V1Job>(@namespace, labelSelector: $"{UnderstudyMetadata.UnderstudyLabel}={understudy}", cancellationToken: cancellationToken)];

    /// <inheritdoc/>
    public async Task UpdateStatusAsync(Understudy understudy, CancellationToken cancellationToken) =>
        await client.UpdateStatusAsync(understudy, cancellationToken);
}
