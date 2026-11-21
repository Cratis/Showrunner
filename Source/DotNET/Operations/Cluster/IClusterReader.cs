// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// Reads the parts of a Kubernetes cluster the checks look at. It is read-only and speaks in the watchdog's own types, so a
/// check is specified with a fake and nothing in the domain depends on the Kubernetes client. A read that fails throws
/// <see cref="ClusterReadException"/>.
/// </summary>
public interface IClusterReader
{
    /// <summary>
    /// Lists pods.
    /// </summary>
    /// <param name="namespace">The namespace, or <see langword="null"/> for all namespaces.</param>
    /// <param name="labelSelector">A label selector, or <see langword="null"/> for all pods.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The pods.</returns>
    Task<IReadOnlyList<PodInfo>> ListPods(string? @namespace, string? labelSelector, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the nodes.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The nodes.</returns>
    Task<IReadOnlyList<NodeInfo>> ListNodes(CancellationToken cancellationToken);

    /// <summary>
    /// Reads the volume usage a node's kubelet reports.
    /// </summary>
    /// <param name="node">The node name.</param>
    /// <param name="timeout">How long to wait for the kubelet.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The usage of the claims mounted on the node.</returns>
    Task<IReadOnlyList<VolumeUsage>> GetVolumeUsage(string node, TimeSpan timeout, CancellationToken cancellationToken);

    /// <summary>
    /// Lists persistent volume claims in all namespaces.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The claims.</returns>
    Task<IReadOnlyList<ClaimInfo>> ListClaims(CancellationToken cancellationToken);

    /// <summary>
    /// Gets a CronJob.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="name">The name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The CronJob, or <see langword="null"/> when it does not exist.</returns>
    Task<CronJobInfo?> GetCronJob(string @namespace, string name, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the Jobs of a namespace.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The Jobs.</returns>
    Task<IReadOnlyList<JobInfo>> ListJobs(string @namespace, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the cert-manager Certificates of all namespaces.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The Certificates.</returns>
    Task<IReadOnlyList<CertificateInfo>> ListCertificates(CancellationToken cancellationToken);

    /// <summary>
    /// Lists the Percona Backup for MongoDB backups of a namespace.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The backups.</returns>
    Task<IReadOnlyList<PbmBackupInfo>> ListPbmBackups(string @namespace, CancellationToken cancellationToken);
}
