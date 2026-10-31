// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Offers;

/// <summary>
/// A price in the smallest unit of its currency, such as cents.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record PriceInMinorUnits(long Value) : ConceptAs<long>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="PriceInMinorUnits"/>.
    /// </summary>
    public static readonly PriceInMinorUnits NotSet = new(0L);

    /// <summary>
    /// Implicitly converts a <see cref="long"/> to a <see cref="PriceInMinorUnits"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator PriceInMinorUnits(long value) => new(value);
}
