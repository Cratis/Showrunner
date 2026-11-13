// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Authoring.Availability;

/// <summary>
/// One thing a product declares, named by what it is and what the model calls it.
/// </summary>
/// <param name="Kind">What kind of thing it is.</param>
/// <param name="Name">What the model calls it: the name of an audience, a block, a value or a persona, or the target of a gate.</param>
public record DeclaredItem(DeclaredItemKind Kind, string Name);
