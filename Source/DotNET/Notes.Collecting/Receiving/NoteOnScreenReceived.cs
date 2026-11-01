// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Collecting.Receiving;

/// <summary>
/// A user gave feedback about the screen itself. It is translated from the public <c>NoteOnScreenGiven</c>.
/// </summary>
/// <param name="AppStore">The event store of the application the note came from.</param>
/// <param name="Application">The application and publication the note was given in.</param>
/// <param name="Giver">The signed-in user who gave it.</param>
/// <param name="Context">What the user was looking at; its screen is the subject.</param>
/// <param name="Kind">What kind of feedback it is.</param>
/// <param name="Text">What the user wrote.</param>
[EventType]
public record NoteOnScreenReceived(AppStoreName AppStore, PublishedApplication Application, NoteGiver Giver, NoteContext Context, NoteKind Kind, NoteText Text);
