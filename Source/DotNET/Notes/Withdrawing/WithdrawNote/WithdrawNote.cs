// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.Notes.Identity;
using Cratis.Showrunner.Notes.Notebook;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Withdrawing.WithdrawNote;

/// <summary>
/// The giver withdraws one of their notes; the people building the application no longer need to act on it.
/// </summary>
/// <param name="NoteId">The note to withdraw.</param>
[Command]
[Authorize]
public record WithdrawNote(NoteId NoteId)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="giver">The signed-in user.</param>
    /// <returns>The <see cref="GiverWithdrewNote"/> event, with the user as its compliance subject.</returns>
    public EventForEventSourceId Handle(ISignedInGiver giver) =>
        NoteGivers.Fact(NoteId, giver.UserId, new GiverWithdrewNote());
}

/// <summary>
/// Validates <see cref="WithdrawNote"/>.
/// </summary>
public class WithdrawNoteValidator : CommandValidator<WithdrawNote>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WithdrawNoteValidator"/> class.
    /// </summary>
    /// <param name="note">The note, or <see langword="null"/> when it does not exist.</param>
    /// <param name="giver">The signed-in user.</param>
    /// <param name="options">The <see cref="NotesOptions"/>.</param>
    public WithdrawNoteValidator(Note? note, ISignedInGiver giver, IOptions<NotesOptions> options)
    {
        RuleFor(c => c.NoteId)
            .MustBeSwitchedOn(options)
            .MustBeGivenBy(note, giver)
            .MustNotBeWithdrawn(note);
    }
}

/// <summary>
/// The giver withdrew a note. It is final: nothing changes the note afterwards. It is published as <c>NoteWithdrawn</c>.
/// </summary>
[EventType]
public record GiverWithdrewNote;
