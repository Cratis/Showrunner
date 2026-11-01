// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Responses;

/// <summary>
/// Why the receiving side decided not to act on a note, as the giver is meant to read it.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record DeclineReason(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="DeclineReason"/>.
    /// </summary>
    public static readonly DeclineReason NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="DeclineReason"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator DeclineReason(string value) => new(value);
}
