// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Alerts;

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// The optional connection between one watchdog pass and Cratis Direct. A pass calls <see cref="Begin"/> before its checks run,
/// <see cref="Observe"/> with what the checks confirmed, and <see cref="End"/> last. None of them throws: a link that cannot work
/// reports it in its status and the pass goes on with its local configuration.
/// </summary>
public interface IWatchdogLink
{
    /// <summary>
    /// Starts the pass: claims the instance, syncs the signed configuration and delivers what is waiting in the outbox.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the link is ready for the checks.</returns>
    Task Begin(CancellationToken cancellationToken);

    /// <summary>
    /// Records what the pass found: queues the facts that changed and runs the remedies Direct governs.
    /// </summary>
    /// <param name="findings">The confirmed findings of the pass by key; empty when everything is healthy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the pass's facts are recorded.</returns>
    Task Observe(IReadOnlyDictionary<string, Finding> findings, CancellationToken cancellationToken);

    /// <summary>
    /// Ends the pass: delivers the outbox, saves the state and reports the status.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the state is saved.</returns>
    Task End(CancellationToken cancellationToken);
}
