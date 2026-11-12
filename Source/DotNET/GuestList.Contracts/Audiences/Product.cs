// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Contracts.Audiences;

/// <summary>
/// Identifies the product an audience decision is for. An operator store serves several products, and a product records
/// only the decisions made for it.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record Product(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="Product"/>.
    /// </summary>
    public static readonly Product NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="Product"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator Product(string value) => new(value);
}
