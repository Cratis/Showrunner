// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Activity;

/// <summary>
/// The number of distinct people who were active.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ActiveUserCount(int Value) : ConceptAs<int>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="ActiveUserCount"/>.
    /// </summary>
    public static readonly ActiveUserCount NotSet = new(0);

    /// <summary>
    /// Implicitly converts a <see cref="int"/> to a <see cref="ActiveUserCount"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ActiveUserCount(int value) => new(value);
}
