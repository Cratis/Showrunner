// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Authoring.Availability;

/// <summary>
/// What kind of thing a product declares.
/// </summary>
public enum DeclaredItemKind
{
    /// <summary>
    /// An audience.
    /// </summary>
    Audience = 0,

    /// <summary>
    /// An availability block.
    /// </summary>
    Availability = 1,

    /// <summary>
    /// A gate.
    /// </summary>
    Gate = 2,

    /// <summary>
    /// A context value.
    /// </summary>
    ContextValue = 3,

    /// <summary>
    /// A persona.
    /// </summary>
    Persona = 4
}
