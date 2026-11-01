// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Context;

/// <summary>
/// The route the user was on when a note was given.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record Route(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="Route"/>.
    /// </summary>
    public static readonly Route NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="Route"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator Route(string value) => new(value);
}
