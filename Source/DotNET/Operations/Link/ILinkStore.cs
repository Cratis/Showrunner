// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// Where the link keeps its state between passes - which are separate, short-lived processes.
/// </summary>
public interface ILinkStore
{
    /// <summary>
    /// Loads the identity, or <see langword="null"/> when the link has never claimed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The identity.</returns>
    Task<LinkIdentity?> LoadIdentity(CancellationToken cancellationToken);

    /// <summary>
    /// Saves the identity.
    /// </summary>
    /// <param name="identity">The identity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when it is stored.</returns>
    Task SaveIdentity(LinkIdentity identity, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the outbox.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outbox, empty when none was saved.</returns>
    Task<OutboxSnapshot> LoadOutbox(CancellationToken cancellationToken);

    /// <summary>
    /// Saves the outbox.
    /// </summary>
    /// <param name="outbox">The outbox.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when it is stored.</returns>
    Task SaveOutbox(OutboxSnapshot outbox, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the memory.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The memory, empty when none was saved.</returns>
    Task<LinkMemory> LoadMemory(CancellationToken cancellationToken);

    /// <summary>
    /// Saves the memory.
    /// </summary>
    /// <param name="memory">The memory.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when it is stored.</returns>
    Task SaveMemory(LinkMemory memory, CancellationToken cancellationToken);

    /// <summary>
    /// Saves the status of the pass.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when it is stored.</returns>
    Task SaveStatus(LinkStatus status, CancellationToken cancellationToken);
}
