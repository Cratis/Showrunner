// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Conversation;

/// <summary>
/// The name the reporter sees for the support person or agent who wrote to them. It is <see cref="NotSet"/> when
/// the desk hides the names of its people, and the application then shows a generic "Support".
/// </summary>
/// <param name="Value">The underlying value.</param>
public record SupportAuthorName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents a hidden or unknown name.
    /// </summary>
    public static readonly SupportAuthorName NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="SupportAuthorName"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator SupportAuthorName(string value) => new(value);
}
