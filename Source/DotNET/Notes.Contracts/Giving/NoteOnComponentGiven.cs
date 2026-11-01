// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Giving;

/// <summary>
/// Published by the application when a user picked a component and wrote text about it. It shares its note id with the
/// <see cref="Components.ComponentPropertyChangeSuggested"/> events of the same picking session.
/// </summary>
/// <param name="TenantId">The tenant the note belongs to.</param>
/// <param name="Application">The application and publication the note was given in.</param>
/// <param name="Giver">The signed-in user who gave it.</param>
/// <param name="Context">What the user was looking at.</param>
/// <param name="Component">The component the user picked.</param>
/// <param name="Kind">What kind of feedback it is.</param>
/// <param name="Text">What the user wrote.</param>
[EventType]
[Public]
public record NoteOnComponentGiven(
    TenantId TenantId,
    PublishedApplication Application,
    NoteGiver Giver,
    NoteContext Context,
    PickedComponent Component,
    NoteKind Kind,
    NoteText Text);
