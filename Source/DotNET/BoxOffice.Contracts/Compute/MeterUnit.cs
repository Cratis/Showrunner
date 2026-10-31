// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Compute;

/// <summary>
/// The unit a meter counts in, such as <c>minutes</c>.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record MeterUnit(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="MeterUnit"/>.
    /// </summary>
    public static readonly MeterUnit NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="MeterUnit"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator MeterUnit(string value) => new(value);
}
