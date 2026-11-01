// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Giving;

/// <summary>
/// What kind of feedback a note is.
/// </summary>
public enum NoteKind
{
    /// <summary>
    /// Something is wrong or does not work as expected.
    /// </summary>
    Problem = 0,

    /// <summary>
    /// A proposal for how something could be better.
    /// </summary>
    Suggestion = 1,

    /// <summary>
    /// The user asks how something works or why it is the way it is.
    /// </summary>
    Question = 2,

    /// <summary>
    /// The user tells what works well.
    /// </summary>
    Praise = 3
}
