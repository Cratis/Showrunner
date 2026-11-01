// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Context;

/// <summary>
/// The id of the screen a note was given on, as the application knows it.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ScreenId(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="ScreenId"/>.
    /// </summary>
    public static readonly ScreenId NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ScreenId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ScreenId(string value) => new(value);
}
