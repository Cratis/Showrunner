// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Offers;

/// <summary>
/// Identifies one offer. It is the event source id of <c>OfferPublished</c> and <c>OfferWithdrawn</c> and never
/// appears in an event payload.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record OfferId(Guid Value) : EventSourceId<Guid>(Value), IGeneratable<OfferId>
{
    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to a <see cref="OfferId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator OfferId(Guid value) => new(value);

    /// <summary>
    /// Creates a new <see cref="OfferId"/>.
    /// </summary>
    /// <returns>A new <see cref="OfferId"/>.</returns>
    public static OfferId New() => new(GenerateValue.Uuid());
}
