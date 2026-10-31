// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Contracts.Announcements;

/// <summary>
/// Identifies the product an announcement is for. Every announcement belongs to exactly one product, and a product receives only the announcements published for it.
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
