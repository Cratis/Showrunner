// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using Cratis.Showrunner.Operations.State;
using k8s;
using k8s.Autorest;
using k8s.Models;

namespace Cratis.Showrunner.Operations.Kubernetes;

/// <summary>
/// An <see cref="IConfigMapStore"/> over the Kubernetes API, in one namespace. The watchdog's ServiceAccount needs get, create
/// and patch on ConfigMaps there and nothing more.
/// </summary>
/// <param name="client">The Kubernetes client.</param>
/// <param name="namespace">The namespace of the ConfigMaps.</param>
public sealed class KubernetesConfigMapStore(IKubernetes client, string @namespace) : IConfigMapStore
{
    /// <inheritdoc/>
    public async Task<string?> Read(string configMap, string key, CancellationToken cancellationToken)
    {
        try
        {
            var map = await client.CoreV1.ReadNamespacedConfigMapAsync(configMap, @namespace, cancellationToken: cancellationToken);
            return map.Data is not null && map.Data.TryGetValue(key, out var value) ? value : null;
        }
        catch (HttpOperationException error) when (error.Response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task Write(string configMap, string key, string value, CancellationToken cancellationToken)
    {
        var body = new V1ConfigMap
        {
            Metadata = new V1ObjectMeta { Name = configMap },
            Data = new Dictionary<string, string>(StringComparer.Ordinal) { [key] = value }
        };

        try
        {
            await client.CoreV1.PatchNamespacedConfigMapAsync(new V1Patch(body, V1Patch.PatchType.MergePatch), configMap, @namespace, cancellationToken: cancellationToken);
        }
        catch (HttpOperationException error) when (error.Response.StatusCode == HttpStatusCode.NotFound)
        {
            await client.CoreV1.CreateNamespacedConfigMapAsync(body, @namespace, cancellationToken: cancellationToken);
        }
    }
}
