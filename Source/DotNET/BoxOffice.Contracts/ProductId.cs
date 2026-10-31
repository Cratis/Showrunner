// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts;

/// <summary>
/// Identifies the managed product. It is the event source id of the product-wide <c>OfferingDeclared</c> event
/// and never appears in an event payload. In Direct it is the product's own identity.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ProductId(Guid Value) : EventSourceId<Guid>(Value), IGeneratable<ProductId>
{
    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to a <see cref="ProductId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ProductId(Guid value) => new(value);

    /// <summary>
    /// Creates a new <see cref="ProductId"/>.
    /// </summary>
    /// <returns>A new <see cref="ProductId"/>.</returns>
    public static ProductId New() => new(GenerateValue.Uuid());
}
