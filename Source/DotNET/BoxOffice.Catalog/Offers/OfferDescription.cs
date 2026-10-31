// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers;

/// <summary>
/// What an offer is for, in a sentence the plan page can show.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record OfferDescription(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="OfferDescription"/>.
    /// </summary>
    public static readonly OfferDescription NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="OfferDescription"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator OfferDescription(string value) => new(value);
}
