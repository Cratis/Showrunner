// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// An <see cref="IRunnerCluster"/> the specifications fill in, which records what was deleted and in what order relative to the state.
/// </summary>
public sealed class FakeRunnerCluster : IRunnerCluster
{
    /// <summary>
    /// Gets the runners.
    /// </summary>
    public List<RunnerInfo> Runners { get; } = [];

    /// <summary>
    /// Gets the runners that were deleted, in order.
    /// </summary>
    public List<string> Deleted { get; } = [];

    /// <summary>
    /// Gets or sets the HTTP status deleting answers with, when it fails.
    /// </summary>
    public int? DeleteFailsWith { get; set; }

    /// <summary>
    /// Gets or sets what happens just before a delete, so a specification can see what had been saved by then.
    /// </summary>
    public Action? BeforeDelete { get; set; }

    /// <inheritdoc/>
    public Task<IReadOnlyList<RunnerInfo>> ListRunners(string @namespace, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<RunnerInfo>>(Runners);

    /// <inheritdoc/>
    public Task DeleteRunner(string @namespace, string name, CancellationToken cancellationToken)
    {
        BeforeDelete?.Invoke();
        Deleted.Add(name);
        return DeleteFailsWith is { } status ? throw new ClusterReadException($"could not delete runner: HTTP {status}", status) : Task.CompletedTask;
    }
}
#endif
