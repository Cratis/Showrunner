// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Context;

/// <summary>
/// The name of the screen a note was given on.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ScreenName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="ScreenName"/>.
    /// </summary>
    public static readonly ScreenName NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ScreenName"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ScreenName(string value) => new(value);
}
