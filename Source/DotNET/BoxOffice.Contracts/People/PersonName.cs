// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.People;

/// <summary>
/// A person's display name.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record PersonName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="PersonName"/>.
    /// </summary>
    public static readonly PersonName NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="PersonName"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator PersonName(string value) => new(value);
}
