// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Listing;

/// <summary>
/// A presentation property change the giver suggested on a note.
/// </summary>
/// <param name="Property">The property.</param>
/// <param name="CurrentValue">The value it has.</param>
/// <param name="SuggestedValue">The value the giver suggests.</param>
public record SuggestedPropertyChange(ComponentProperty Property, PropertyValue CurrentValue, PropertyValue SuggestedValue);
