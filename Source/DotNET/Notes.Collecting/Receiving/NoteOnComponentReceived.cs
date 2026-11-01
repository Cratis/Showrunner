// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Collecting.Receiving;

/// <summary>
/// A user gave feedback about a component they picked. It is translated from the public <c>NoteOnComponentGiven</c>.
/// </summary>
/// <param name="AppStore">The event store of the application the note came from.</param>
/// <param name="Application">The application and publication the note was given in.</param>
/// <param name="Giver">The signed-in user who gave it.</param>
/// <param name="Context">What the user was looking at.</param>
/// <param name="Component">The component the user picked.</param>
/// <param name="Kind">What kind of feedback it is.</param>
/// <param name="Text">What the user wrote.</param>
[EventType]
public record NoteOnComponentReceived(AppStoreName AppStore, PublishedApplication Application, NoteGiver Giver, NoteContext Context, PickedComponent Component, NoteKind Kind, NoteText Text);
