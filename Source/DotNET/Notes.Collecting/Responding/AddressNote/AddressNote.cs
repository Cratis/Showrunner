// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Collecting.Notebook;

namespace Cratis.Showrunner.Notes.Collecting.Responding.AddressNote;

/// <summary>
/// The builder tells that a version of the application that acts on a note was rolled out. It is published to the
/// application as <c>NoteAddressed</c>.
/// </summary>
/// <param name="NoteId">The note that was addressed.</param>
/// <param name="Version">The version that acts on the note.</param>
[Command]
[Authorize]
public record AddressNote(NoteId NoteId, AddressedInVersion Version)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <returns>The <see cref="OperatorAddressedNote"/> event.</returns>
    public OperatorAddressedNote Handle() => new(Version);
}

/// <summary>
/// Validates <see cref="AddressNote"/>.
/// </summary>
public class AddressNoteValidator : CommandValidator<AddressNote>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddressNoteValidator"/> class.
    /// </summary>
    /// <param name="note">The note, or <see langword="null"/> when it was not received.</param>
    public AddressNoteValidator(ReceivedNote? note)
    {
        RuleFor(c => c.NoteId)
            .MustBeAnOpenNote(note);
        RuleFor(c => (string)c.Version)
            .NotEmpty().WithMessage("Name the version that addresses the note.")
            .Must((command, version) => note is null || note.AddressedInVersion != command.Version).WithMessage("The note is already addressed in this version.");
    }
}

/// <summary>
/// The builder addressed a note in a version. It is published as <c>NoteAddressed</c>.
/// </summary>
/// <param name="Version">The version that acts on the note.</param>
[EventType]
public record OperatorAddressedNote(AddressedInVersion Version);
