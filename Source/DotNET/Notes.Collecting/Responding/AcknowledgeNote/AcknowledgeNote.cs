// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Collecting.Notebook;

namespace Cratis.Showrunner.Notes.Collecting.Responding.AcknowledgeNote;

/// <summary>
/// The builder acknowledges a note it received, so the giver can see that it arrived. It is published to the application
/// as <c>NoteAcknowledged</c>.
/// </summary>
/// <param name="NoteId">The note to acknowledge.</param>
[Command]
[Authorize]
public record AcknowledgeNote(NoteId NoteId)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <returns>The <see cref="OperatorAcknowledgedNote"/> event.</returns>
    public OperatorAcknowledgedNote Handle() => new();
}

/// <summary>
/// Validates <see cref="AcknowledgeNote"/>.
/// </summary>
public class AcknowledgeNoteValidator : CommandValidator<AcknowledgeNote>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AcknowledgeNoteValidator"/> class.
    /// </summary>
    /// <param name="note">The note, or <see langword="null"/> when it was not received.</param>
    public AcknowledgeNoteValidator(ReceivedNote? note)
    {
        RuleFor(c => c.NoteId)
            .MustBeAnOpenNote(note)
            .Must(_ => note is not { IsAcknowledged: true }).WithMessage("The note is already acknowledged.");
    }
}

/// <summary>
/// The builder acknowledged a note. It is published as <c>NoteAcknowledged</c>.
/// </summary>
[EventType]
public record OperatorAcknowledgedNote;
