// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Giving;

/// <summary>
/// Published by the application when a user picked the screen itself as the subject of a note, as opposed to the context
/// the screen happens to be in.
/// </summary>
/// <param name="TenantId">The tenant the note belongs to.</param>
/// <param name="Application">The application and publication the note was given in.</param>
/// <param name="Giver">The signed-in user who gave it.</param>
/// <param name="Context">What the user was looking at; <c>Context.Screen</c> is the subject.</param>
/// <param name="Kind">What kind of feedback it is.</param>
/// <param name="Text">What the user wrote.</param>
[EventType]
[Public]
public record NoteOnScreenGiven(
    TenantId TenantId,
    PublishedApplication Application,
    NoteGiver Giver,
    NoteContext Context,
    NoteKind Kind,
    NoteText Text);
