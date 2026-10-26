// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Conversation;

/// <summary>
/// The text of a reply support sends the reporter that does not ask anything.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ReplyText(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="ReplyText"/>.
    /// </summary>
    public static readonly ReplyText NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ReplyText"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ReplyText(string value) => new(value);
}
