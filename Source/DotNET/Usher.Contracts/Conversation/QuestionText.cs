// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Conversation;

/// <summary>
/// The text of a question support asks the reporter.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record QuestionText(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="QuestionText"/>.
    /// </summary>
    public static readonly QuestionText NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="QuestionText"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator QuestionText(string value) => new(value);
}
