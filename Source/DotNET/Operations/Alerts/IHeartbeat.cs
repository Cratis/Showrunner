// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Alerts;

/// <summary>
/// The external dead-man's switch: a watchdog that stops running reports nothing, so something outside the cluster must notice
/// the silence.
/// </summary>
public interface IHeartbeat
{
    /// <summary>
    /// Reports that the run completed, or that it failed.
    /// </summary>
    /// <param name="failed">Whether the run failed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the report is delivered. It throws when it cannot be delivered.</returns>
    Task Ping(bool failed, CancellationToken cancellationToken);
}
