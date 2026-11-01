// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Context;

/// <summary>
/// The screen the user was on when they gave a note.
/// </summary>
/// <param name="ScreenId">The id of the screen.</param>
/// <param name="ScreenName">The name of the screen.</param>
/// <param name="ScreenPath">The path of the screen.</param>
public record NoteScreen(ScreenId ScreenId, ScreenName ScreenName, ScreenPath ScreenPath)
{
    /// <summary>
    /// Represents a screen that could not be resolved: every part is <c>NotSet</c>.
    /// </summary>
    public static readonly NoteScreen NotSet = new(ScreenId.NotSet, ScreenName.NotSet, ScreenPath.NotSet);
}
