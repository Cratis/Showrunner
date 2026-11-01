// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Responses;

/// <summary>
/// The version of the application that acts on a note.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record AddressedInVersion(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="AddressedInVersion"/>.
    /// </summary>
    public static readonly AddressedInVersion NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="AddressedInVersion"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator AddressedInVersion(string value) => new(value);
}
