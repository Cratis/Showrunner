// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.Notes.Contracts.Transport;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Transport;

/// <summary>
/// Sends the notes in the outbox of a namespace to the collector, in order and in batches.
/// </summary>
/// <remarks>
/// <para>
/// The application keeps no checkpoint of its own. The first time it relays a namespace, and the first time after a round
/// that failed, it asks the collector for the highest outbox sequence number it stored and carries on after it. The
/// collector deduplicates on the sequence number, so a batch that arrived but was never acknowledged, or one sent twice
/// by two instances of the application, is stored once.
/// </para>
/// <para>
/// Only the Notes contract events the application publishes are sent: the outbox holds other parts' events too.
/// </para>
/// </remarks>
/// <param name="collector">The collector.</param>
/// <param name="options">Notes' options.</param>
public class OutboxRelay(ICollectorClient collector, IOptions<NotesOptions> options)
{
    readonly Dictionary<string, ulong> _next = [];

    /// <summary>
    /// Gets the contract events the application publishes.
    /// </summary>
    public static IReadOnlyList<Type> Published { get; } =
    [
        typeof(NoteOnContextGiven),
        typeof(NoteOnSlicesGiven),
        typeof(NoteOnScreenGiven),
        typeof(NoteOnComponentGiven),
        typeof(ComponentPropertyChangeSuggested),
        typeof(NoteWithdrawn)
    ];

    /// <summary>
    /// Sends what the collector has not stored yet from the outbox of a namespace.
    /// </summary>
    /// <param name="eventStore">The application's event store in the namespace.</param>
    /// <param name="cancellationToken">Stops the round when the host shuts down.</param>
    /// <returns>The number of notes sent.</returns>
    public async Task<int> Relay(IEventStore eventStore, CancellationToken cancellationToken = default)
    {
        var source = eventStore.Namespace.Value;
        try
        {
            if (!_next.TryGetValue(source, out var next))
            {
                var position = await collector.GetPosition(source, cancellationToken);
                next = position is { } stored ? stored + 1 : 0;
            }

            var types = Published.Select(eventStore.EventTypes.GetEventTypeFor).ToArray();
            var pending = await eventStore.GetEventSequence(EventSequenceId.Outbox)
                .GetFromSequenceNumber(new EventSequenceNumber(next), filterEventTypes: types);

            var sent = 0;
            foreach (var batch in pending.Where(_ => _.Context.SequenceNumber.Value >= next).Chunk(options.Value.RelayBatchSize))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await collector.Send(source, [.. batch.Select(Describe)], cancellationToken);
                next = batch[^1].Context.SequenceNumber.Value + 1;
                _next[source] = next;
                sent += batch.Length;
            }

            _next[source] = next;
            return sent;
        }
        catch
        {
            // Ask the collector again after a failure: it knows what it stored, whatever happened to the request.
            _next.Remove(source);
            throw;
        }
    }

    static PublishedNoteEvent Describe(AppendedEvent appended)
    {
        var context = appended.Context;
        return new(
            context.SequenceNumber.Value,
            new(context.EventType.Id.Value, context.EventType.Generation.Value),
            context.Occurred,
            context.EventSourceId.Value,
            JsonSerializer.SerializeToElement(appended.Content, appended.Content.GetType(), NotesTransportJson.Options),
            context.CorrelationId.Value,
            context.Subject.IsSet ? context.Subject.Value : null);
    }
}
