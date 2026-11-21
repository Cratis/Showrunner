// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Showrunner.Operations.Cluster;
using k8s;
using k8s.Autorest;

namespace Cratis.Showrunner.Operations.Kubernetes;

/// <summary>
/// An <see cref="IClusterReader"/> that reads the cluster with the Kubernetes client. It only reads.
/// </summary>
/// <param name="client">The Kubernetes client.</param>
public sealed class KubernetesClusterReader(IKubernetes client) : IClusterReader
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<PodInfo>> ListPods(string? @namespace, string? labelSelector, CancellationToken cancellationToken) =>
        Guard("pods", async () =>
        {
            var pods = @namespace is null
                ? await client.CoreV1.ListPodForAllNamespacesAsync(labelSelector: labelSelector, cancellationToken: cancellationToken)
                : await client.CoreV1.ListNamespacedPodAsync(@namespace, labelSelector: labelSelector, cancellationToken: cancellationToken);
            return (IReadOnlyList<PodInfo>)[.. pods.Items.Select(KubernetesMapper.Pod)];
        });

    /// <inheritdoc/>
    public Task<IReadOnlyList<NodeInfo>> ListNodes(CancellationToken cancellationToken) =>
        Guard("nodes", async () => (IReadOnlyList<NodeInfo>)[.. (await client.CoreV1.ListNodeAsync(cancellationToken: cancellationToken)).Items.Select(KubernetesMapper.Node)]);

    /// <inheritdoc/>
    public Task<IReadOnlyList<VolumeUsage>> GetVolumeUsage(string node, TimeSpan timeout, CancellationToken cancellationToken) =>
        Guard("kubelet summary", async () =>
        {
            using var timeLimit = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeLimit.CancelAfter(timeout);
            await using var stream = await client.CoreV1.ConnectGetNodeProxyWithPathAsync(node, "stats/summary", cancellationToken: timeLimit.Token);
            using var text = new StreamReader(stream);
            return KubernetesMapper.VolumeUsage(await text.ReadToEndAsync(timeLimit.Token));
        });

    /// <inheritdoc/>
    public Task<IReadOnlyList<ClaimInfo>> ListClaims(CancellationToken cancellationToken) =>
        Guard("persistent volume claims", async () =>
        {
            var claims = await client.CoreV1.ListPersistentVolumeClaimForAllNamespacesAsync(cancellationToken: cancellationToken);
            return (IReadOnlyList<ClaimInfo>)[.. claims.Items.Select(claim => new ClaimInfo(claim.Metadata.NamespaceProperty, claim.Metadata.Name, claim.Status?.Phase ?? "Unknown"))];
        });

    /// <inheritdoc/>
    public async Task<CronJobInfo?> GetCronJob(string @namespace, string name, CancellationToken cancellationToken)
    {
        try
        {
            return KubernetesMapper.CronJob(await client.BatchV1.ReadNamespacedCronJobAsync(name, @namespace, cancellationToken: cancellationToken));
        }
        catch (HttpOperationException error) when (error.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (HttpOperationException error)
        {
            throw new ClusterReadException($"could not read CronJob {name}: HTTP {(int)error.Response.StatusCode}", (int)error.Response.StatusCode, error);
        }
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<JobInfo>> ListJobs(string @namespace, CancellationToken cancellationToken) =>
        Guard("jobs", async () => (IReadOnlyList<JobInfo>)[.. (await client.BatchV1.ListNamespacedJobAsync(@namespace, cancellationToken: cancellationToken)).Items.Select(KubernetesMapper.Job)]);

    /// <inheritdoc/>
    public Task<IReadOnlyList<CertificateInfo>> ListCertificates(CancellationToken cancellationToken) =>
        Guard("certificates", async () =>
        {
            var list = await client.CustomObjects.ListClusterCustomObjectAsync("cert-manager.io", "v1", "certificates", cancellationToken: cancellationToken);
            return KubernetesMapper.Certificates(JsonSerializer.SerializeToElement(list));
        });

    /// <inheritdoc/>
    public Task<IReadOnlyList<PbmBackupInfo>> ListPbmBackups(string @namespace, CancellationToken cancellationToken) =>
        Guard("PBM backups", async () =>
        {
            var list = await client.CustomObjects.ListNamespacedCustomObjectAsync("psmdb.percona.com", "v1", @namespace, "perconaservermongodbbackups", cancellationToken: cancellationToken);
            return KubernetesMapper.PbmBackups(JsonSerializer.SerializeToElement(list));
        });

    static async Task<T> Guard<T>(string what, Func<Task<T>> read)
    {
        try
        {
            return await read();
        }
        catch (HttpOperationException error)
        {
            throw new ClusterReadException($"could not read {what}: HTTP {(int)error.Response.StatusCode}", (int)error.Response.StatusCode, error);
        }
    }
}
