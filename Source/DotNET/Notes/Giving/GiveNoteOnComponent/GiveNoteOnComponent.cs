// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.Notes.Identity;
using Cratis.Showrunner.Notes.Notebook;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Giving.GiveNoteOnComponent;

/// <summary>
/// The user picked a component and wrote text about it. The property changes they suggested in the same picking session
/// share its note id. Who gives it is taken from the signed-in identity, never from the request.
/// </summary>
/// <param name="NoteId">The id of the note: new, or the one the picking session's property changes were suggested on.</param>
/// <param name="Context">What the user is looking at.</param>
/// <param name="Component">The component the user picked.</param>
/// <param name="Kind">What kind of feedback it is.</param>
/// <param name="Text">What the user writes.</param>
[Command]
[Authorize]
public record GiveNoteOnComponent(NoteId NoteId, NoteContext Context, PickedComponent Component, NoteKind Kind, NoteText Text)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="giver">The signed-in user.</param>
    /// <param name="options">The <see cref="NotesOptions"/> naming the application.</param>
    /// <returns>The <see cref="GiverGaveNoteOnComponent"/> event, with the user as its compliance subject.</returns>
    public EventForEventSourceId Handle(ISignedInGiver giver, IOptions<NotesOptions> options) =>
        NoteGivers.Fact(NoteId, giver.UserId, new GiverGaveNoteOnComponent(PublishedApplications.Of(options.Value), giver.Giver, Context, Component, Kind, Text));
}

/// <summary>
/// Validates <see cref="GiveNoteOnComponent"/>.
/// </summary>
public class GiveNoteOnComponentValidator : CommandValidator<GiveNoteOnComponent>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GiveNoteOnComponentValidator"/> class.
    /// </summary>
    /// <param name="note">The note with the command's id, <see langword="null"/> when it does not exist yet.</param>
    /// <param name="giver">The signed-in user.</param>
    /// <param name="options">The <see cref="NotesOptions"/>.</param>
    public GiveNoteOnComponentValidator(Note? note, ISignedInGiver giver, IOptions<NotesOptions> options)
    {
        RuleFor(c => c.NoteId).MustBeSwitchedOn(options).MustBeSignedIn(giver).MustBeOpenForText(note, giver);
        RuleFor(c => c.Context).MustBeReasonable();
        RuleFor(c => c.Component).MustBeReasonable();
        RuleFor(c => c.Kind).IsInEnum().WithMessage("Choose what kind of note this is.");
        RuleFor(c => (string)c.Text).MustBeNoteText();
    }
}

/// <summary>
/// The user gave feedback about a component they picked. It is published as <c>NoteOnComponentGiven</c>.
/// </summary>
/// <param name="Application">The application and publication the note was given in.</param>
/// <param name="Giver">Who gave it.</param>
/// <param name="Context">What the user was looking at.</param>
/// <param name="Component">The component the user picked.</param>
/// <param name="Kind">What kind of feedback it is.</param>
/// <param name="Text">What the user wrote.</param>
[EventType]
public record GiverGaveNoteOnComponent(PublishedApplication Application, NoteGiver Giver, NoteContext Context, PickedComponent Component, NoteKind Kind, NoteText Text);
