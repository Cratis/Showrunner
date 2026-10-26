// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Conversation;

/// <summary>
/// The reporter's answer to a question support asked.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record Answer(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="Answer"/>.
    /// </summary>
    public static readonly Answer NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to an <see cref="Answer"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator Answer(string value) => new(value);
}
