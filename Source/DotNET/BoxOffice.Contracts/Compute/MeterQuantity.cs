// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Compute;

/// <summary>
/// An amount counted by a meter, in the meter's unit.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record MeterQuantity(long Value) : ConceptAs<long>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="MeterQuantity"/>.
    /// </summary>
    public static readonly MeterQuantity NotSet = new(0L);

    /// <summary>
    /// Implicitly converts a <see cref="long"/> to a <see cref="MeterQuantity"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator MeterQuantity(long value) => new(value);
}
