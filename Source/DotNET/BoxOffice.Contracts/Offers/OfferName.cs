// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Offers;

/// <summary>
/// The name of an offer as a person reads it.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record OfferName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="OfferName"/>.
    /// </summary>
    public static readonly OfferName NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="OfferName"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator OfferName(string value) => new(value);
}
