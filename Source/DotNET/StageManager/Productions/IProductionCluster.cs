// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Performances;

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// What the Production reconciliation needs from the cluster. It is the one place the cluster is touched, so everything else can be specified without one.
/// </summary>
public interface IProductionCluster
{
    /// <summary>
    /// Reads one value of a Secret. The value is for the caller to use and never to log.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="name">The name of the Secret.</param>
    /// <param name="key">The key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The value, or null when the Secret or the key does not exist.</returns>
    Task<string?> GetSecretValueAsync(string @namespace, string name, string key, CancellationToken cancellationToken);

    /// <summary>
    /// Reads whether a custom resource definition is installed, which tells whether the operator that owns it is.
    /// </summary>
    /// <param name="name">The name of the definition, such as <c>certificates.cert-manager.io</c>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True when it is installed.</returns>
    Task<bool> HasDefinitionAsync(string name, CancellationToken cancellationToken);

    /// <summary>
    /// Creates the resource, or replaces it when it exists.
    /// </summary>
    /// <typeparam name="TResource">The type of resource.</typeparam>
    /// <param name="desired">The resource as it should be.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the resource is applied.</returns>
    Task ApplyAsync<TResource>(TResource desired, CancellationToken cancellationToken)
        where TResource : IKubernetesObject<V1ObjectMeta>;

    /// <summary>
    /// Reads the state the Percona operator reports for a managed database.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="name">The name of the PerconaServerMongoDB.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The state, or null when the database does not exist or reports none yet.</returns>
    Task<string?> GetMongoDBStateAsync(string @namespace, string name, CancellationToken cancellationToken);

    /// <summary>
    /// Reads how far a StatefulSet has come.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="name">The name of the StatefulSet.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The readiness, or null when the StatefulSet does not exist.</returns>
    Task<DeploymentReadiness?> GetStatefulSetReadinessAsync(string @namespace, string name, CancellationToken cancellationToken);

    /// <summary>
    /// Writes the status of a Production.
    /// </summary>
    /// <param name="production">The Production, with its status filled in.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the status is written.</returns>
    Task UpdateStatusAsync(Production production, CancellationToken cancellationToken);
}
