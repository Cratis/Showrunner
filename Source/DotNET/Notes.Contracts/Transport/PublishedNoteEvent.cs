// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Showrunner.Notes.Contracts.Transport;

/// <summary>
/// One contract event an application relays to the collector: what the application's outbox holds, with where it sits in
/// the outbox.
/// </summary>
/// <remarks>
/// The wire format is camelCase JSON. <c>correlationId</c> and <c>subject</c> are optional on the wire; the collector
/// records the events it translates with them when present.
/// </remarks>
/// <param name="SequenceNumber">The sequence number of the event in the outbox of the application. The collector deduplicates on it per publication.</param>
/// <param name="EventType">The contract event type.</param>
/// <param name="Occurred">When the application recorded the event.</param>
/// <param name="EventSourceId">The note the event is about.</param>
/// <param name="Content">The contract event, as the JSON the contracts serialize to.</param>
/// <param name="CorrelationId">The correlation the event was recorded under, if it had one.</param>
/// <param name="Subject">The compliance subject of the event, if it had one.</param>
public record PublishedNoteEvent(
    ulong SequenceNumber,
    PublishedEventType EventType,
    DateTimeOffset Occurred,
    string EventSourceId,
    JsonElement Content,
    Guid? CorrelationId = null,
    string? Subject = null);
