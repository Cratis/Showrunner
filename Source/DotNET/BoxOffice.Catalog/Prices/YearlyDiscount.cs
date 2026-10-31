// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Prices;

/// <summary>
/// The discount, as a percentage between 0 and 100, for paying a year at once instead of twelve months.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record YearlyDiscount(decimal Value) : ConceptAs<decimal>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="YearlyDiscount"/>.
    /// </summary>
    public static readonly YearlyDiscount NotSet = new(0m);

    /// <summary>
    /// Implicitly converts a <see cref="decimal"/> to a <see cref="YearlyDiscount"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator YearlyDiscount(decimal value) => new(value);
}
