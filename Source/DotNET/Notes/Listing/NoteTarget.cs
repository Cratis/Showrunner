// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Listing;

/// <summary>
/// What a note is about.
/// </summary>
public enum NoteTarget
{
    /// <summary>
    /// What was on screen when the note was given.
    /// </summary>
    Context = 0,

    /// <summary>
    /// One or more slices the user picked.
    /// </summary>
    Slices = 1,

    /// <summary>
    /// The screen itself.
    /// </summary>
    Screen = 2,

    /// <summary>
    /// A component the user picked, possibly with suggested property changes.
    /// </summary>
    Component = 3
}
