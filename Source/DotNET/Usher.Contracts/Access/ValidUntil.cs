// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Access;

/// <summary>
/// The fixed point in time a support access grant ends. The reporter picks a duration; the application turns it
/// into this time when the grant is recorded, so the fact carries a time and not a duration.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ValidUntil(DateTimeOffset Value) : ConceptAs<DateTimeOffset>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="DateTimeOffset"/> to a <see cref="ValidUntil"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ValidUntil(DateTimeOffset value) => new(value);
}
