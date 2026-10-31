// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Offers;

/// <summary>
/// An ISO 4217 currency code, such as <c>EUR</c>.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record CurrencyCode(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="CurrencyCode"/>.
    /// </summary>
    public static readonly CurrencyCode NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="CurrencyCode"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator CurrencyCode(string value) => new(value);
}
