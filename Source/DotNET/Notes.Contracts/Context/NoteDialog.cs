// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Context;

/// <summary>
/// The dialog that was open when the user gave a note.
/// </summary>
/// <param name="DialogTitle">The title of the dialog.</param>
/// <param name="Location">Where in the application the dialog belongs, as Arc resolves it.</param>
public record NoteDialog(DialogTitle DialogTitle, SliceLocation Location)
{
    /// <summary>
    /// Represents that no dialog was open.
    /// </summary>
    public static readonly NoteDialog NoDialog = new(DialogTitle.NotSet, SliceLocation.NotSet);
}
