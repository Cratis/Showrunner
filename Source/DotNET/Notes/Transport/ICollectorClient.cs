// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Contracts.Transport;

namespace Cratis.Showrunner.Notes.Transport;

/// <summary>
/// Defines the calls the application makes to the collector.
/// </summary>
public interface ICollectorClient
{
    /// <summary>
    /// Asks which outbox sequence number the collector stored last for a source.
    /// </summary>
    /// <param name="source">The name of the event log the notes are relayed from; the namespace.</param>
    /// <param name="cancellationToken">Stops the call when the host shuts down.</param>
    /// <returns>The highest sequence number stored, or <see langword="null"/> when it stored none.</returns>
    Task<ulong?> GetPosition(string source, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends notes, in outbox order.
    /// </summary>
    /// <param name="source">The name of the event log the notes are relayed from; the namespace.</param>
    /// <param name="events">The notes.</param>
    /// <param name="cancellationToken">Stops the call when the host shuts down.</param>
    /// <returns>An awaitable task.</returns>
    /// <exception cref="CollectorRequestFailed">The collector refused the request.</exception>
    Task Send(string source, IReadOnlyList<PublishedNoteEvent> events, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the answers after a cursor.
    /// </summary>
    /// <param name="after">The cursor of the last answer seen; zero for all.</param>
    /// <param name="cancellationToken">Stops the call when the host shuts down.</param>
    /// <returns>The answers in cursor order.</returns>
    Task<IReadOnlyList<NoteResponse>> GetResponses(ulong after, CancellationToken cancellationToken = default);
}
