// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Collecting.Transport;

/// <summary>
/// The collector stored the notes of a publication up to an outbox sequence number. It is the position the application asks
/// for after it starts, and is recorded under the event source of the publication and the source of the notes.
/// </summary>
/// <param name="Publication">The publication.</param>
/// <param name="Source">The event log of the application the notes were relayed from; the empty string for the default.</param>
/// <param name="SequenceNumber">The highest outbox sequence number stored.</param>
[EventType]
public record NoteSequenceCollected(PublicationId Publication, string Source, ulong SequenceNumber);
