// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Collecting.Notebook;

namespace Cratis.Showrunner.Notes.Collecting.Responding.DeclineNote;

/// <summary>
/// The builder decides not to act on a note. It is published to the application as <c>NoteDeclined</c>.
/// </summary>
/// <param name="NoteId">The note to decline.</param>
/// <param name="Reason">Why it will not act on the note, as the giver is meant to read it.</param>
[Command]
[Authorize]
public record DeclineNote(NoteId NoteId, DeclineReason Reason)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <returns>The <see cref="OperatorDeclinedNote"/> event.</returns>
    public OperatorDeclinedNote Handle() => new(Reason);
}

/// <summary>
/// Validates <see cref="DeclineNote"/>.
/// </summary>
public class DeclineNoteValidator : CommandValidator<DeclineNote>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeclineNoteValidator"/> class.
    /// </summary>
    /// <param name="note">The note, or <see langword="null"/> when it was not received.</param>
    public DeclineNoteValidator(ReceivedNote? note)
    {
        RuleFor(c => c.NoteId)
            .MustBeAnOpenNote(note)
            .Must(_ => note is not { IsDeclined: true }).WithMessage("The note is already declined.");
        RuleFor(c => (string)c.Reason).NotEmpty().WithMessage("Tell the giver why you will not act on the note.");
    }
}

/// <summary>
/// The builder declined a note. It is published as <c>NoteDeclined</c>.
/// </summary>
/// <param name="Reason">Why it will not act on the note.</param>
[EventType]
public record OperatorDeclinedNote(DeclineReason Reason);
