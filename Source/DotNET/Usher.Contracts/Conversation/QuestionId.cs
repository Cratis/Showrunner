// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Conversation;

/// <summary>
/// Identifies one question support asked on an issue. The desk mints it with <see cref="QuestionAsked"/> and the
/// reporter's <see cref="QuestionAnswered"/> carries it back, so an answer always targets a specific question.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record QuestionId(Guid Value) : ConceptAs<Guid>(Value), IGeneratable<QuestionId>
{
    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to a <see cref="QuestionId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator QuestionId(Guid value) => new(value);

    /// <summary>
    /// Creates a new <see cref="QuestionId"/>.
    /// </summary>
    /// <returns>A new <see cref="QuestionId"/>.</returns>
    public static QuestionId New() => new(GenerateValue.Uuid());
}
