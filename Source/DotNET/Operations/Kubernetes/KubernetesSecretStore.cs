// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Text;
using Cratis.Showrunner.Operations.State;
using k8s;
using k8s.Autorest;
using k8s.Models;

namespace Cratis.Showrunner.Operations.Kubernetes;

/// <summary>
/// Reads and writes one value of a Secret in the watchdog's own namespace. The watchdog's service account needs get, create and
/// patch on Secrets there, and only when the Direct link is enabled.
/// </summary>
/// <param name="client">The Kubernetes client.</param>
/// <param name="namespace">The namespace the watchdog runs in.</param>
public sealed class KubernetesSecretStore(IKubernetes client, string @namespace) : ISecretStore
{
    /// <inheritdoc/>
    public async Task<string?> Read(string secret, string key, CancellationToken cancellationToken)
    {
        try
        {
            var found = await client.CoreV1.ReadNamespacedSecretAsync(secret, @namespace, cancellationToken: cancellationToken);
            return found.Data is not null && found.Data.TryGetValue(key, out var value) ? Encoding.UTF8.GetString(value) : null;
        }
        catch (HttpOperationException error) when (error.Response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task Write(string secret, string key, string value, CancellationToken cancellationToken)
    {
        var body = new V1Secret
        {
            Metadata = new V1ObjectMeta { Name = secret },
            Data = new Dictionary<string, byte[]>(StringComparer.Ordinal) { [key] = Encoding.UTF8.GetBytes(value) }
        };
        try
        {
            await client.CoreV1.PatchNamespacedSecretAsync(new V1Patch(body, V1Patch.PatchType.MergePatch), secret, @namespace, cancellationToken: cancellationToken);
        }
        catch (HttpOperationException error) when (error.Response.StatusCode == HttpStatusCode.NotFound)
        {
            await client.CoreV1.CreateNamespacedSecretAsync(body, @namespace, cancellationToken: cancellationToken);
        }
    }
}
