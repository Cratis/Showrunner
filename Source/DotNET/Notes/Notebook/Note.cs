// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Giving.GiveNoteOnComponent;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnContext;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnScreen;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnSlices;
using Cratis.Showrunner.Notes.Giving.SuggestComponentPropertyChange;
using Cratis.Showrunner.Notes.Withdrawing.WithdrawNote;

namespace Cratis.Showrunner.Notes.Notebook;

/// <summary>
/// The state the giver's commands decide on, one per note. It is the write side's own model, never shown to anyone: the
/// giver's view is <c>MyNote</c>.
/// </summary>
/// <remarks>
/// It is a self-reducing read model. With read protection on, a command's validator reads it fresh from the event log and
/// the read guards the append, so a note is withdrawn once and its text given once, also under concurrent requests. Once
/// withdrawn, a note keeps its state: an answer that crossed the withdrawal does not bring it back.
/// </remarks>
/// <param name="Id">The note.</param>
/// <param name="GivenBy">Who gave the note.</param>
/// <param name="TextGiven">Whether the giver already wrote the text of the note; suggested property changes alone do not.</param>
/// <param name="IsWithdrawn">Whether the giver withdrew the note.</param>
public record Note(NoteId Id, GiverUserId GivenBy, bool TextGiven, bool IsWithdrawn)
{
    /// <summary>
    /// Opens the note from the text the giver wrote about the context.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static Note Reduce(GiverGaveNoteOnContext @event, Note? current, EventContext context) => Text(current, context, @event.Giver);

    /// <summary>
    /// Opens the note from the text the giver wrote about slices.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static Note Reduce(GiverGaveNoteOnSlices @event, Note? current, EventContext context) => Text(current, context, @event.Giver);

    /// <summary>
    /// Opens the note from the text the giver wrote about the screen.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static Note Reduce(GiverGaveNoteOnScreen @event, Note? current, EventContext context) => Text(current, context, @event.Giver);

    /// <summary>
    /// Opens the note from the text the giver wrote about a component.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static Note Reduce(GiverGaveNoteOnComponent @event, Note? current, EventContext context) => Text(current, context, @event.Giver);

    /// <summary>
    /// Opens the note from a property change the giver suggested; the text may follow.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static Note Reduce(GiverSuggestedComponentPropertyChange @event, Note? current, EventContext context) =>
        current ?? Open(context, @event.Giver, false);

    /// <summary>
    /// Records that the giver withdrew the note.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static Note? Reduce(GiverWithdrewNote @event, Note? current) => current is null ? null : current with { IsWithdrawn = true };

    static Note Text(Note? current, EventContext context, NoteGiver giver) =>
        (current ?? Open(context, giver, true)) with { TextGiven = true };

    static Note Open(EventContext context, NoteGiver giver, bool textGiven) =>
        new(new NoteId(Guid.Parse(context.EventSourceId.Value)), giver.UserId, textGiven, false);
}
