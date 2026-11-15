// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Productions;

namespace Cratis.Showrunner.StageManager.Understudies;

/// <summary>
/// What the Understudy reconciliation needs from the cluster. It is the one place the cluster is touched, so everything else can be specified without one.
/// </summary>
public interface IUnderstudyCluster
{
    /// <summary>
    /// Reads a Production.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="name">The name of the Production.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The Production, or null when it does not exist.</returns>
    Task<Production?> GetProductionAsync(string @namespace, string name, CancellationToken cancellationToken);

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
    /// Creates the resource, or replaces it when it exists.
    /// </summary>
    /// <typeparam name="TResource">The type of resource.</typeparam>
    /// <param name="desired">The resource as it should be.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the resource is applied.</returns>
    Task ApplyAsync<TResource>(TResource desired, CancellationToken cancellationToken)
        where TResource : IKubernetesObject<V1ObjectMeta>;

    /// <summary>
    /// Creates the resource unless one with the same name exists.
    /// </summary>
    /// <typeparam name="TResource">The type of resource.</typeparam>
    /// <param name="desired">The resource.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True when it was created; false when it existed.</returns>
    Task<bool> TryCreateAsync<TResource>(TResource desired, CancellationToken cancellationToken)
        where TResource : IKubernetesObject<V1ObjectMeta>;

    /// <summary>
    /// Lists the jobs an Understudy created.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="understudy">The name of the Understudy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The jobs.</returns>
    Task<IReadOnlyList<V1Job>> ListJobsAsync(string @namespace, string understudy, CancellationToken cancellationToken);

    /// <summary>
    /// Writes the status of an Understudy.
    /// </summary>
    /// <param name="understudy">The Understudy, with its status filled in.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the status is written.</returns>
    Task UpdateStatusAsync(Understudy understudy, CancellationToken cancellationToken);
}
