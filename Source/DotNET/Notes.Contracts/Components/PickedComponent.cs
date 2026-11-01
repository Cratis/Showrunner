// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Components;

/// <summary>
/// A component of the application the user picked on screen.
/// </summary>
/// <param name="ComponentName">The name of the component.</param>
/// <param name="ComponentId">The id of the component.</param>
/// <param name="Part">The part of the component that was picked, <see cref="ComponentPart.NotSet"/> for the component as a whole.</param>
/// <param name="Ancestors">The components it sits in, nearest first; may be empty.</param>
/// <param name="Location">Where in the application the component is.</param>
public record PickedComponent(
    ComponentName ComponentName,
    ComponentId ComponentId,
    ComponentPart Part,
    IEnumerable<ComponentName> Ancestors,
    SliceLocation Location);
