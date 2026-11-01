// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Context;

/// <summary>
/// The path of the screen a note was given on, as the screen is reached in the application.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ScreenPath(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="ScreenPath"/>.
    /// </summary>
    public static readonly ScreenPath NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ScreenPath"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ScreenPath(string value) => new(value);
}
