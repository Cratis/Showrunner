// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Contracts.Availability;

/// <summary>
/// The version of the product build whose availability model is declared, as its informational version reads.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ModelVersion(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="ModelVersion"/>.
    /// </summary>
    public static readonly ModelVersion NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ModelVersion"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ModelVersion(string value) => new(value);
}
