// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Components;

/// <summary>
/// Published by the application when a user changed a presentation property of a component in the picker. There is one
/// event per property, and it shares its note id with the <see cref="Giving.NoteOnComponentGiven"/> of the same picking
/// session.
/// </summary>
/// <param name="TenantId">The tenant the note belongs to.</param>
/// <param name="Application">The application and publication the suggestion was given in.</param>
/// <param name="Giver">The signed-in user who gave it.</param>
/// <param name="Context">What the user was looking at.</param>
/// <param name="Component">The component the user picked.</param>
/// <param name="Property">The property the user changed.</param>
/// <param name="CurrentValue">The value the property has.</param>
/// <param name="SuggestedValue">The value the user suggests.</param>
[EventType]
[Public]
public record ComponentPropertyChangeSuggested(
    TenantId TenantId,
    PublishedApplication Application,
    NoteGiver Giver,
    NoteContext Context,
    PickedComponent Component,
    ComponentProperty Property,
    PropertyValue CurrentValue,
    PropertyValue SuggestedValue);
