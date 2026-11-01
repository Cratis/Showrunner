// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Collecting.Receiving;

/// <summary>
/// The name of the event store of the application a note came from.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record AppStoreName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="AppStoreName"/>.
    /// </summary>
    public static readonly AppStoreName NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to an <see cref="AppStoreName"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator AppStoreName(string value) => new(value);
}
