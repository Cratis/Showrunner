// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// An <see cref="IHeartbeat"/> that records its pings, and fails on demand.
/// </summary>
public sealed class RecordingHeartbeat : IHeartbeat
{
    /// <summary>
    /// Gets the pings; <see langword="true"/> is a failure report.
    /// </summary>
    public List<bool> Pings { get; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether pinging throws.
    /// </summary>
    public bool Fail { get; set; }

    /// <inheritdoc/>
    public Task Ping(bool failed, CancellationToken cancellationToken)
    {
        Pings.Add(failed);
        return Fail ? throw new InvalidOperationException("external heartbeat delivery failed after two attempts") : Task.CompletedTask;
    }
}
#endif
