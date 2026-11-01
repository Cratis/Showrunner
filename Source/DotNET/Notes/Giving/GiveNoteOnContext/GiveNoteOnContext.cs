// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.Notes.Identity;
using Cratis.Showrunner.Notes.Notebook;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Giving.GiveNoteOnContext;

/// <summary>
/// The user gives feedback about what is on screen now: the default action of the feedback button. Who gives it is taken
/// from the signed-in identity, never from the request.
/// </summary>
/// <param name="NoteId">The id of the new note, chosen by the caller so a retried command does not give a second note.</param>
/// <param name="Context">What the user is looking at.</param>
/// <param name="Kind">What kind of feedback it is.</param>
/// <param name="Text">What the user writes.</param>
[Command]
[Authorize]
public record GiveNoteOnContext(NoteId NoteId, NoteContext Context, NoteKind Kind, NoteText Text)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="giver">The signed-in user.</param>
    /// <param name="options">The <see cref="NotesOptions"/> naming the application.</param>
    /// <returns>The <see cref="GiverGaveNoteOnContext"/> event, with the user as its compliance subject.</returns>
    public EventForEventSourceId Handle(ISignedInGiver giver, IOptions<NotesOptions> options) =>
        NoteGivers.Fact(NoteId, giver.UserId, new GiverGaveNoteOnContext(PublishedApplications.Of(options.Value), giver.Giver, Context, Kind, Text));
}

/// <summary>
/// Validates <see cref="GiveNoteOnContext"/>.
/// </summary>
public class GiveNoteOnContextValidator : CommandValidator<GiveNoteOnContext>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GiveNoteOnContextValidator"/> class.
    /// </summary>
    /// <param name="note">The note with the command's id, <see langword="null"/> when it does not exist yet.</param>
    /// <param name="giver">The signed-in user.</param>
    /// <param name="options">The <see cref="NotesOptions"/>.</param>
    public GiveNoteOnContextValidator(Note? note, ISignedInGiver giver, IOptions<NotesOptions> options)
    {
        RuleFor(c => c.NoteId).MustBeSwitchedOn(options).MustBeSignedIn(giver).MustBeOpenForText(note, giver);
        RuleFor(c => c.Context).MustBeReasonable();
        RuleFor(c => c.Kind).IsInEnum().WithMessage("Choose what kind of note this is.");
        RuleFor(c => (string)c.Text).MustBeNoteText();
    }
}

/// <summary>
/// The user gave feedback about what is on screen. It is published as <c>NoteOnContextGiven</c>.
/// </summary>
/// <param name="Application">The application and publication the note was given in.</param>
/// <param name="Giver">Who gave it.</param>
/// <param name="Context">What the user was looking at.</param>
/// <param name="Kind">What kind of feedback it is.</param>
/// <param name="Text">What the user wrote.</param>
[EventType]
public record GiverGaveNoteOnContext(PublishedApplication Application, NoteGiver Giver, NoteContext Context, NoteKind Kind, NoteText Text);
