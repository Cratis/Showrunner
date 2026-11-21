// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Showrunner.Operations.Cluster;
using k8s;
using k8s.Autorest;

namespace Cratis.Showrunner.Operations.Kubernetes;

/// <summary>
/// An <see cref="IRunnerCluster"/> that lists and deletes <c>ephemeralrunners.actions.github.com</c> with the Kubernetes client.
/// It does nothing else to the cluster.
/// </summary>
/// <param name="client">The Kubernetes client.</param>
public sealed class KubernetesRunnerCluster(IKubernetes client) : IRunnerCluster
{
    const string Group = "actions.github.com";
    const string Version = "v1alpha1";
    const string Plural = "ephemeralrunners";

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RunnerInfo>> ListRunners(string @namespace, CancellationToken cancellationToken)
    {
        try
        {
            var list = await client.CustomObjects.ListNamespacedCustomObjectAsync(Group, Version, @namespace, Plural, cancellationToken: cancellationToken);
            return KubernetesMapper.Runners(JsonSerializer.SerializeToElement(list));
        }
        catch (HttpOperationException error)
        {
            throw new ClusterReadException($"could not read runners: HTTP {(int)error.Response.StatusCode}", (int)error.Response.StatusCode, error);
        }
    }

    /// <inheritdoc/>
    public async Task DeleteRunner(string @namespace, string name, CancellationToken cancellationToken)
    {
        try
        {
            await client.CustomObjects.DeleteNamespacedCustomObjectAsync(Group, Version, @namespace, Plural, name, cancellationToken: cancellationToken);
        }
        catch (HttpOperationException error)
        {
            throw new ClusterReadException($"could not delete runner: HTTP {(int)error.Response.StatusCode}", (int)error.Response.StatusCode, error);
        }
    }
}
