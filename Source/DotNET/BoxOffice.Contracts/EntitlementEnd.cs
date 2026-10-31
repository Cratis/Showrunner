// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts;

/// <summary>
/// The last day an entitlement (a module grant, a module preview or a seat capacity) holds. <see cref="Never"/>
/// means it has no end date.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record EntitlementEnd(DateOnly Value) : ConceptAs<DateOnly>(Value)
{
    /// <summary>
    /// An entitlement that never ends.
    /// </summary>
    public static readonly EntitlementEnd Never = new(DateOnly.MaxValue);

    /// <summary>
    /// Implicitly converts a <see cref="DateOnly"/> to a <see cref="EntitlementEnd"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator EntitlementEnd(DateOnly value) => new(value);
}
