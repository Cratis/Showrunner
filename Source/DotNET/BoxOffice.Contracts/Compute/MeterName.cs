// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Compute;

/// <summary>
/// The name of a meter as a person reads it.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record MeterName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="MeterName"/>.
    /// </summary>
    public static readonly MeterName NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="MeterName"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator MeterName(string value) => new(value);
}
