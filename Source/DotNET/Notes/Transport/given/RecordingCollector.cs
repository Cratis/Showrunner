// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Contracts.Transport;

namespace Cratis.Showrunner.Notes.Transport.given;

/// <summary>
/// A collector that keeps what the application sent it and answers what a specification says.
/// </summary>
public class RecordingCollector : ICollectorClient
{
    readonly List<(string Source, IReadOnlyList<PublishedNoteEvent> Notes)> _sent = [];

    /// <summary>
    /// Gets or sets the position the collector reports.
    /// </summary>
    public ulong? Position { get; set; }

    /// <summary>
    /// Gets or sets what to fail every call with.
    /// </summary>
    public Exception? FailWith { get; set; }

    /// <summary>
    /// Gets the answers the collector has.
    /// </summary>
    public List<NoteResponse> Answers { get; } = [];

    /// <summary>
    /// Gets how many times the position was asked.
    /// </summary>
    public int PositionAsked { get; private set; }

    /// <summary>
    /// Gets the requests the application sent.
    /// </summary>
    public IReadOnlyList<(string Source, IReadOnlyList<PublishedNoteEvent> Notes)> Sent => _sent;

    /// <summary>
    /// Gets every note sent, in order.
    /// </summary>
    public IReadOnlyList<PublishedNoteEvent> AllNotes => [.. _sent.SelectMany(_ => _.Notes)];

    /// <inheritdoc/>
    public Task<ulong?> GetPosition(string source, CancellationToken cancellationToken = default)
    {
        PositionAsked++;
        return FailWith is { } failure ? Task.FromException<ulong?>(failure) : Task.FromResult(Position);
    }

    /// <inheritdoc/>
    public Task Send(string source, IReadOnlyList<PublishedNoteEvent> events, CancellationToken cancellationToken = default)
    {
        if (FailWith is { } failure)
        {
            return Task.FromException(failure);
        }

        _sent.Add((source, events));
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<NoteResponse>> GetResponses(ulong after, CancellationToken cancellationToken = default) =>
        FailWith is { } failure
            ? Task.FromException<IReadOnlyList<NoteResponse>>(failure)
            : Task.FromResult<IReadOnlyList<NoteResponse>>([.. Answers.Where(_ => _.Cursor > after)]);
}
#endif
