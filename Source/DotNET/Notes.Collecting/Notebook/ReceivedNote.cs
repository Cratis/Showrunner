// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Collecting.Receiving;
using Cratis.Showrunner.Notes.Collecting.Responding.AcknowledgeNote;
using Cratis.Showrunner.Notes.Collecting.Responding.AddressNote;
using Cratis.Showrunner.Notes.Collecting.Responding.DeclineNote;

namespace Cratis.Showrunner.Notes.Collecting.Notebook;

/// <summary>
/// The state the collecting commands decide on, one per note received in a tenant. It is the write side's own model; the
/// host builds the views of notes it needs from the private facts.
/// </summary>
/// <remarks>
/// It is a self-reducing read model. With read protection on, a command's validator reads it fresh from the event log and
/// the read guards the append. Once withdrawn, a note keeps its state: nothing is acted on it afterwards.
/// </remarks>
/// <param name="Id">The note.</param>
/// <param name="IsWithdrawn">Whether the giver withdrew the note.</param>
/// <param name="IsAcknowledged">Whether the note was acknowledged.</param>
/// <param name="AddressedInVersion">The version the note was last addressed in; <see cref="AddressedInVersion.NotSet"/> when it was not.</param>
/// <param name="IsDeclined">Whether the note is declined: the last decision on it was to decline.</param>
public record ReceivedNote(NoteId Id, bool IsWithdrawn, bool IsAcknowledged, AddressedInVersion AddressedInVersion, bool IsDeclined)
{
    /// <summary>
    /// Receives a note about what was on screen.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReceivedNote Reduce(NoteOnContextReceived @event, ReceivedNote? current, EventContext context) => current ?? ReceivedNoteTransitions.Open(context);

    /// <summary>
    /// Receives a note about slices.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReceivedNote Reduce(NoteOnSlicesReceived @event, ReceivedNote? current, EventContext context) => current ?? ReceivedNoteTransitions.Open(context);

    /// <summary>
    /// Receives a note about the screen.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReceivedNote Reduce(NoteOnScreenReceived @event, ReceivedNote? current, EventContext context) => current ?? ReceivedNoteTransitions.Open(context);

    /// <summary>
    /// Receives a note about a component.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReceivedNote Reduce(NoteOnComponentReceived @event, ReceivedNote? current, EventContext context) => current ?? ReceivedNoteTransitions.Open(context);

    /// <summary>
    /// Receives a suggested property change; it opens a note that has no text yet.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReceivedNote Reduce(ComponentPropertyChangeReceived @event, ReceivedNote? current, EventContext context) => current ?? ReceivedNoteTransitions.Open(context);

    /// <summary>
    /// Records that the giver withdrew the note.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static ReceivedNote? Reduce(NoteWithdrawalReceived @event, ReceivedNote? current) => current is null ? null : current with { IsWithdrawn = true };

    /// <summary>
    /// Records that the note was acknowledged.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static ReceivedNote? Reduce(OperatorAcknowledgedNote @event, ReceivedNote? current) => current is null ? null : current with { IsAcknowledged = true };

    /// <summary>
    /// Records that the note was addressed in a version; it is no longer declined.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static ReceivedNote? Reduce(OperatorAddressedNote @event, ReceivedNote? current) =>
        current is null ? null : current with { AddressedInVersion = @event.Version, IsDeclined = false };

    /// <summary>
    /// Records that the note was declined; it is no longer addressed.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static ReceivedNote? Reduce(OperatorDeclinedNote @event, ReceivedNote? current) =>
        current is null ? null : current with { AddressedInVersion = AddressedInVersion.NotSet, IsDeclined = true };
}

/// <summary>
/// The transitions the reducers of <see cref="ReceivedNote"/> share.
/// </summary>
internal static class ReceivedNoteTransitions
{
    /// <summary>
    /// Opens a note as received.
    /// </summary>
    /// <param name="context">The event context.</param>
    /// <returns>The opened note.</returns>
    internal static ReceivedNote Open(EventContext context) =>
        new(new NoteId(Guid.Parse(context.EventSourceId.Value)), false, false, AddressedInVersion.NotSet, false);
}
