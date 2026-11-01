// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Linq;
using System.Reactive.Subjects;
using Cratis.Arc.Queries.ModelBound;
using Cratis.Chronicle.ReadModels;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnComponent;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnContext;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnScreen;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnSlices;
using Cratis.Showrunner.Notes.Giving.SuggestComponentPropertyChange;
using Cratis.Showrunner.Notes.Identity;
using Cratis.Showrunner.Notes.Receiving;
using Cratis.Showrunner.Notes.Withdrawing.WithdrawNote;
using ReactiveSubject = System.Reactive.Subjects.Subject;

namespace Cratis.Showrunner.Notes.Listing;

/// <summary>
/// One of the user's notes, as the list of their notes shows it, with where it is: given, acknowledged, addressed,
/// declined or withdrawn.
/// </summary>
/// <remarks>
/// It is a self-reducing read model. Once withdrawn, a note stays withdrawn: an answer that crossed the withdrawal does not
/// change it. An acknowledgement moves a note on only from given, so it never takes back an addressed or declined note.
/// What arrives about a note the application never recorded changes nothing.
/// </remarks>
/// <param name="Id">The note.</param>
/// <param name="GivenBy">Who gave the note.</param>
/// <param name="Target">What the note is about.</param>
/// <param name="Kind">What kind of feedback it is.</param>
/// <param name="Text">What the giver wrote; empty on a note that has suggested property changes only.</param>
/// <param name="Location">Where in the application the giver was.</param>
/// <param name="Screen">The screen the giver was on.</param>
/// <param name="Slices">The slices the giver picked; empty unless the note is about slices.</param>
/// <param name="Component">The name of the component the giver picked; <see cref="ComponentName.NotSet"/> unless the note is about a component.</param>
/// <param name="PropertyChanges">The property changes the giver suggested.</param>
/// <param name="Status">Where the note is.</param>
/// <param name="StatusDetail">The version that addresses the note or why it is declined; empty otherwise.</param>
/// <param name="GivenAt">When the giver gave it.</param>
/// <param name="LastActivityAt">When anything last happened on it.</param>
[ReadModel]
public record MyNote(
    NoteId Id,
    GiverUserId GivenBy,
    NoteTarget Target,
    NoteKind Kind,
    MyNoteText Text,
    SliceLocation Location,
    NoteScreen Screen,
    IEnumerable<SliceLocation> Slices,
    ComponentName Component,
    IEnumerable<SuggestedPropertyChange> PropertyChanges,
    NoteStatus Status,
    NoteStatusDetail StatusDetail,
    DateTimeOffset GivenAt,
    DateTimeOffset LastActivityAt)
{
    /// <summary>
    /// Observes the notes the signed-in user gave, in the current tenant, most recent activity first.
    /// </summary>
    /// <param name="readModels">The <see cref="IReadModels"/> of the current tenant's event store namespace.</param>
    /// <param name="giver">The signed-in user.</param>
    /// <returns>The user's notes, again every time one changes.</returns>
    [Authorize]
    public static ISubject<IEnumerable<MyNote>> MyNotes(IReadModels readModels, ISignedInGiver giver)
    {
        if (!giver.IsSignedIn)
        {
            return new BehaviorSubject<IEnumerable<MyNote>>([]);
        }

        var userId = giver.UserId;
        var source = readModels.Observe<MyNote>(note => note.GivenBy == userId);
        return ReactiveSubject.Create<IEnumerable<MyNote>>(
            System.Reactive.Observer.Create<IEnumerable<MyNote>>(_ => { }, source.OnError, source.OnCompleted),
            source.Select(notes => (IEnumerable<MyNote>)[.. notes.OrderByDescending(note => note.LastActivityAt)]));
    }

    /// <summary>
    /// Records a note about what was on screen.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static MyNote Reduce(GiverGaveNoteOnContext @event, MyNote? current, EventContext context) =>
        MyNoteTransitions.Written(current, context, @event.Giver, NoteTarget.Context, @event.Kind, @event.Text, @event.Context) with { Slices = [] };

    /// <summary>
    /// Records a note about slices.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static MyNote Reduce(GiverGaveNoteOnSlices @event, MyNote? current, EventContext context) =>
        MyNoteTransitions.Written(current, context, @event.Giver, NoteTarget.Slices, @event.Kind, @event.Text, @event.Context) with { Slices = [.. @event.Slices] };

    /// <summary>
    /// Records a note about the screen.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static MyNote Reduce(GiverGaveNoteOnScreen @event, MyNote? current, EventContext context) =>
        MyNoteTransitions.Written(current, context, @event.Giver, NoteTarget.Screen, @event.Kind, @event.Text, @event.Context) with { Slices = [] };

    /// <summary>
    /// Records a note about a component.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static MyNote Reduce(GiverGaveNoteOnComponent @event, MyNote? current, EventContext context) =>
        MyNoteTransitions.Written(current, context, @event.Giver, NoteTarget.Component, @event.Kind, @event.Text, @event.Context) with
        {
            Slices = [],
            Component = @event.Component.ComponentName
        };

    /// <summary>
    /// Records a property change the giver suggested.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static MyNote Reduce(GiverSuggestedComponentPropertyChange @event, MyNote? current, EventContext context)
    {
        var opened = current ?? MyNoteTransitions.Open(context, @event.Giver, NoteTarget.Component, NoteKind.Suggestion, MyNoteText.NotSet, @event.Context);
        return MyNoteTransitions.Touch(opened, context) with
        {
            Component = @event.Component.ComponentName,
            PropertyChanges = [.. opened.PropertyChanges.Where(change => change.Property != @event.Property), new(@event.Property, @event.CurrentValue, @event.SuggestedValue)]
        };
    }

    /// <summary>
    /// Records that the giver withdrew the note.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static MyNote? Reduce(GiverWithdrewNote @event, MyNote? current, EventContext context) =>
        MyNoteTransitions.Move(current, context, NoteStatus.Withdrawn, NoteStatusDetail.NotSet);

    /// <summary>
    /// Records that the receiving side recorded the note.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static MyNote? Reduce(ReceiverAcknowledgedNote @event, MyNote? current, EventContext context) =>
        current is { Status: NoteStatus.Given } ? MyNoteTransitions.Move(current, context, NoteStatus.Acknowledged, NoteStatusDetail.NotSet) : current;

    /// <summary>
    /// Records that a version that acts on the note was rolled out.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static MyNote? Reduce(ReceiverAddressedNote @event, MyNote? current, EventContext context) =>
        MyNoteTransitions.Move(current, context, NoteStatus.Addressed, (string)@event.Version);

    /// <summary>
    /// Records that the receiving side decided not to act on the note.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static MyNote? Reduce(ReceiverDeclinedNote @event, MyNote? current, EventContext context) =>
        MyNoteTransitions.Move(current, context, NoteStatus.Declined, (string)@event.Reason);
}

/// <summary>
/// The transitions the reducers of <see cref="MyNote"/> share.
/// </summary>
internal static class MyNoteTransitions
{
    /// <summary>
    /// Records the text of a note, opening the note when it is not open.
    /// </summary>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <param name="giver">Who gave the note.</param>
    /// <param name="target">What the note is about.</param>
    /// <param name="kind">What kind of feedback it is.</param>
    /// <param name="text">What the giver wrote.</param>
    /// <param name="noteContext">What the giver was looking at.</param>
    /// <returns>The next state.</returns>
    internal static MyNote Written(MyNote? current, EventContext context, NoteGiver giver, NoteTarget target, NoteKind kind, NoteText text, NoteContext noteContext)
    {
        var opened = current ?? Open(context, giver, target, kind, (string)text, noteContext);
        return Touch(opened, context) with
        {
            Target = target,
            Kind = kind,
            Text = (string)text,
            Location = noteContext.Location,
            Screen = noteContext.Screen
        };
    }

    /// <summary>
    /// Opens a note as given.
    /// </summary>
    /// <param name="context">The event context.</param>
    /// <param name="giver">Who gave the note.</param>
    /// <param name="target">What the note is about.</param>
    /// <param name="kind">What kind of feedback it is.</param>
    /// <param name="text">What the giver wrote.</param>
    /// <param name="noteContext">What the giver was looking at.</param>
    /// <returns>The opened note.</returns>
    internal static MyNote Open(EventContext context, NoteGiver giver, NoteTarget target, NoteKind kind, MyNoteText text, NoteContext noteContext) => new(
        new NoteId(Guid.Parse(context.EventSourceId.Value)),
        giver.UserId,
        target,
        kind,
        text,
        noteContext.Location,
        noteContext.Screen,
        [],
        ComponentName.NotSet,
        [],
        NoteStatus.Given,
        NoteStatusDetail.NotSet,
        context.Occurred,
        context.Occurred);

    /// <summary>
    /// Moves the last activity of a note to when the event happened.
    /// </summary>
    /// <param name="note">The note.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    internal static MyNote Touch(MyNote note, EventContext context) => note with { LastActivityAt = context.Occurred };

    /// <summary>
    /// Moves a note to a status, unless it is withdrawn.
    /// </summary>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <param name="status">The status to move to.</param>
    /// <param name="detail">What the receiving side said about it.</param>
    /// <returns>The next state.</returns>
    internal static MyNote? Move(MyNote? current, EventContext context, NoteStatus status, NoteStatusDetail detail) =>
        current is null || current.Status == NoteStatus.Withdrawn
            ? current
            : Touch(current, context) with { Status = status, StatusDetail = detail };
}
