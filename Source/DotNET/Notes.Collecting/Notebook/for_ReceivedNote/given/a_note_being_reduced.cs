// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.Notes.Collecting.given;
using Cratis.Showrunner.Notes.Collecting.Receiving;

namespace Cratis.Showrunner.Notes.Collecting.Notebook.for_ReceivedNote.given;

/// <summary>
/// The pieces the specifications of the <see cref="ReceivedNote"/> reducers share.
/// </summary>
public class a_note_being_reduced : Specification
{
    protected static readonly NoteId TheNote = NoteId.New();

    protected static readonly ReceivedNote Opened = new(TheNote, false, false, AddressedInVersion.NotSet, false);

    protected static EventContext Context => EventContext.From(
        "Studio",
        "fjord-logistics",
        EventType.Unknown,
        EventSourceType.Default,
        TheNote,
        EventStreamType.All,
        EventStreamId.Default,
        0UL,
        Cratis.Execution.CorrelationId.New(),
        new DateTimeOffset(2026, 10, 26, 15, 0, 0, TimeSpan.Zero));

    protected static NoteOnSlicesReceived OnSlices => new(
        ReceivedNotes.AppStore,
        ReceivedNotes.Application,
        ReceivedNotes.Giver,
        ReceivedNotes.Context,
        [ReceivedNotes.Location],
        NoteKind.Problem,
        "The invoice is not sent.");

    protected static NoteOnScreenReceived OnScreen => new(
        ReceivedNotes.AppStore,
        ReceivedNotes.Application,
        ReceivedNotes.Giver,
        ReceivedNotes.Context,
        NoteKind.Problem,
        "The invoice is not sent.");

    protected static NoteOnComponentReceived OnComponent => new(
        ReceivedNotes.AppStore,
        ReceivedNotes.Application,
        ReceivedNotes.Giver,
        ReceivedNotes.Context,
        ReceivedNotes.Component,
        NoteKind.Problem,
        "The invoice is not sent.");

    protected static ComponentPropertyChangeReceived PropertyChange => new(
        ReceivedNotes.AppStore,
        ReceivedNotes.Application,
        ReceivedNotes.Giver,
        ReceivedNotes.Context,
        ReceivedNotes.Component,
        "label",
        "Send",
        "Send invoice");
}
#endif
