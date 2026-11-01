// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Collecting.Receiving;

/// <summary>
/// A user suggested a new value for a presentation property of a component. It is translated from the public
/// <c>ComponentPropertyChangeSuggested</c>.
/// </summary>
/// <param name="AppStore">The event store of the application the suggestion came from.</param>
/// <param name="Application">The application and publication the suggestion was given in.</param>
/// <param name="Giver">The signed-in user who gave it.</param>
/// <param name="Context">What the user was looking at.</param>
/// <param name="Component">The component the user picked.</param>
/// <param name="Property">The property the user changed.</param>
/// <param name="CurrentValue">The value the property has.</param>
/// <param name="SuggestedValue">The value the user suggests.</param>
[EventType]
public record ComponentPropertyChangeReceived(
    AppStoreName AppStore,
    PublishedApplication Application,
    NoteGiver Giver,
    NoteContext Context,
    PickedComponent Component,
    ComponentProperty Property,
    PropertyValue CurrentValue,
    PropertyValue SuggestedValue);
