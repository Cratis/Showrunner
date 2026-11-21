// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// An <see cref="IClusterReader"/> the specifications fill in. Anything not filled in is an empty list; a source named in
/// <see cref="Failing"/> throws <see cref="ClusterReadException"/>.
/// </summary>
public sealed class FakeClusterReader : IClusterReader
{
    /// <summary>
    /// Gets the pods.
    /// </summary>
    public List<PodInfo> Pods { get; } = [];

    /// <summary>
    /// Gets the nodes.
    /// </summary>
    public List<NodeInfo> Nodes { get; } = [];

    /// <summary>
    /// Gets the volume usage per node. A node without an entry has an unreadable summary.
    /// </summary>
    public Dictionary<string, List<VolumeUsage>> Usage { get; } = [];

    /// <summary>
    /// Gets the claims.
    /// </summary>
    public List<ClaimInfo> Claims { get; } = [];

    /// <summary>
    /// Gets the CronJobs by name.
    /// </summary>
    public Dictionary<string, CronJobInfo> CronJobs { get; } = [];

    /// <summary>
    /// Gets the Jobs.
    /// </summary>
    public List<JobInfo> Jobs { get; } = [];

    /// <summary>
    /// Gets the Certificates.
    /// </summary>
    public List<CertificateInfo> Certificates { get; } = [];

    /// <summary>
    /// Gets the PBM backups.
    /// </summary>
    public List<PbmBackupInfo> Backups { get; } = [];

    /// <summary>
    /// Gets the sources that fail to read, by the name of the interface method, with the HTTP status they answer with.
    /// </summary>
    public Dictionary<string, int?> Failing { get; } = [];

    /// <summary>
    /// Gets the timeouts the volume usage of each node was asked with.
    /// </summary>
    public List<(string Node, TimeSpan Timeout)> SummaryReads { get; } = [];

    /// <summary>
    /// Gets the namespaces the pods were listed for.
    /// </summary>
    public List<string?> PodNamespaces { get; } = [];

    /// <summary>
    /// Gets or sets how many times the first read of a node's summary fails before it succeeds.
    /// </summary>
    public int SummaryFailuresBeforeSuccess { get; set; }

    /// <inheritdoc/>
    public Task<IReadOnlyList<PodInfo>> ListPods(string? @namespace, string? labelSelector, CancellationToken cancellationToken)
    {
        Guard(nameof(ListPods));
        PodNamespaces.Add(@namespace);
        var pairs = (labelSelector ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries).Select(pair => pair.Split('=')).ToList();
        return Task.FromResult<IReadOnlyList<PodInfo>>([.. Pods.Where(pod => (@namespace is null || pod.Namespace == @namespace)
            && pairs.TrueForAll(pair => pod.Labels is not null && pod.Labels.TryGetValue(pair[0], out var value) && value == pair[1]))]);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<NodeInfo>> ListNodes(CancellationToken cancellationToken)
    {
        Guard(nameof(ListNodes));
        return Task.FromResult<IReadOnlyList<NodeInfo>>(Nodes);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<VolumeUsage>> GetVolumeUsage(string node, TimeSpan timeout, CancellationToken cancellationToken)
    {
        SummaryReads.Add((node, timeout));
        if (SummaryReads.Count(read => read.Node == node) <= SummaryFailuresBeforeSuccess || !Usage.TryGetValue(node, out var usage))
        {
            throw new ClusterReadException($"the summary of {node} timed out");
        }

        return Task.FromResult<IReadOnlyList<VolumeUsage>>(usage);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ClaimInfo>> ListClaims(CancellationToken cancellationToken)
    {
        Guard(nameof(ListClaims));
        return Task.FromResult<IReadOnlyList<ClaimInfo>>(Claims);
    }

    /// <inheritdoc/>
    public Task<CronJobInfo?> GetCronJob(string @namespace, string name, CancellationToken cancellationToken)
    {
        Guard(nameof(GetCronJob));
        return Task.FromResult(CronJobs.GetValueOrDefault(name));
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<JobInfo>> ListJobs(string @namespace, CancellationToken cancellationToken)
    {
        Guard(nameof(ListJobs));
        return Task.FromResult<IReadOnlyList<JobInfo>>(Jobs);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<CertificateInfo>> ListCertificates(CancellationToken cancellationToken)
    {
        Guard(nameof(ListCertificates));
        return Task.FromResult<IReadOnlyList<CertificateInfo>>(Certificates);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<PbmBackupInfo>> ListPbmBackups(string @namespace, CancellationToken cancellationToken)
    {
        Guard(nameof(ListPbmBackups));
        return Task.FromResult<IReadOnlyList<PbmBackupInfo>>(Backups);
    }

    void Guard(string source)
    {
        if (Failing.TryGetValue(source, out var status))
        {
            throw new ClusterReadException($"{source} failed", status);
        }
    }
}
#endif
