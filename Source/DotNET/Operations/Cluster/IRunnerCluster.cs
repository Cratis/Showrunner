// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// The runner scale set resources the runner watchdog reads and removes. It is the only thing the watchdog is allowed to delete:
/// ephemeral runners, and nothing else. A failure throws <see cref="ClusterReadException"/>.
/// </summary>
public interface IRunnerCluster
{
    /// <summary>
    /// Lists the ephemeral runners of a namespace.
    /// </summary>
    /// <param name="namespace">The namespace of the runner scale sets.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The runners.</returns>
    Task<IReadOnlyList<RunnerInfo>> ListRunners(string @namespace, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an ephemeral runner; the scale set creates a fresh one. A runner that is already gone is not an error and
    /// is reported as a <see cref="ClusterReadException"/> with status 404, which the caller ignores.
    /// </summary>
    /// <param name="namespace">The namespace of the runner.</param>
    /// <param name="name">The runner name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the runner is deleted.</returns>
    Task DeleteRunner(string @namespace, string name, CancellationToken cancellationToken);
}
