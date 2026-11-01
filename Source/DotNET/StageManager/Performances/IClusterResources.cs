// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// What the Performance reconciliation needs from the cluster. It is the one place the cluster is touched, so everything else can be specified without one.
/// </summary>
public interface IClusterResources
{
    /// <summary>
    /// Reads one value of a ConfigMap.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="name">The name of the ConfigMap.</param>
    /// <param name="key">The key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The value, or null when the ConfigMap or the key does not exist.</returns>
    Task<string?> GetConfigMapValueAsync(string @namespace, string name, string key, CancellationToken cancellationToken);

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
    /// Gets the name of the connection Secret a Production reports in its status.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="name">The name of the Production.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The Secret name, or null when the Production does not exist or reports none yet.</returns>
    Task<string?> GetProductionConnectionSecretNameAsync(string @namespace, string name, CancellationToken cancellationToken);

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
    /// Reads how far a Deployment has come.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="name">The name of the Deployment.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The readiness, or null when the Deployment does not exist.</returns>
    Task<DeploymentReadiness?> GetDeploymentReadinessAsync(string @namespace, string name, CancellationToken cancellationToken);

    /// <summary>
    /// Writes the status of a Performance.
    /// </summary>
    /// <param name="performance">The Performance, with its status filled in.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the status is written.</returns>
    Task UpdateStatusAsync(Performance performance, CancellationToken cancellationToken);

    /// <summary>
    /// Reads whether a ConfigMap holds a package.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="name">The name of the ConfigMap.</param>
    /// <param name="key">The key of the package, in data or in binary data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>What is known, or null when the ConfigMap does not exist.</returns>
    Task<PackageState?> GetPackageStateAsync(string @namespace, string name, string key, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a resource. Deleting one that does not exist is not an error.
    /// </summary>
    /// <typeparam name="TResource">The type of resource.</typeparam>
    /// <param name="namespace">The namespace.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the resource is gone.</returns>
    Task DeleteAsync<TResource>(string @namespace, string name, CancellationToken cancellationToken)
        where TResource : IKubernetesObject<V1ObjectMeta>;

    /// <summary>
    /// Sets the number of replicas of a Deployment. A Deployment that does not exist is left alone.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="name">The name of the Deployment.</param>
    /// <param name="replicas">The number of replicas.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the Deployment is scaled.</returns>
    Task ScaleDeploymentAsync(string @namespace, string name, int replicas, CancellationToken cancellationToken);

    /// <summary>
    /// Reads what the pods of a Performance use right now, from the metrics API.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="performance">The name of the Performance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The usage, or null when the metrics API is not available or has no pod of the Performance yet.</returns>
    Task<UsageSample?> GetUsageAsync(string @namespace, string performance, CancellationToken cancellationToken);
}
