// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Link;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// An <see cref="IDeployments"/> that records the restarts it was asked for.
/// </summary>
public sealed class RecordingDeployments : IDeployments
{
    /// <summary>
    /// Gets the restarts, as <c>namespace/name</c>.
    /// </summary>
    public List<string> Restarted { get; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the restart fails.
    /// </summary>
    public bool Fail { get; set; }

    /// <inheritdoc/>
    public Task RolloutRestart(string @namespace, string name, CancellationToken cancellationToken)
    {
        if (Fail)
        {
            throw new InvalidOperationException("forbidden");
        }

        Restarted.Add($"{@namespace}/{name}");
        return Task.CompletedTask;
    }
}
#endif
