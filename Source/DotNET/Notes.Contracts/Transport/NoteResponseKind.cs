// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Transport;

/// <summary>
/// What the receiving side did with a note.
/// </summary>
public enum NoteResponseKind
{
    /// <summary>
    /// The receiving side recorded the note.
    /// </summary>
    Acknowledged = 0,

    /// <summary>
    /// A version that acts on the note was rolled out.
    /// </summary>
    Addressed = 1,

    /// <summary>
    /// The receiving side decided not to act on the note.
    /// </summary>
    Declined = 2
}
