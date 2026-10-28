// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Conversation;

/// <summary>
/// Published by the desk when support asks the reporter something. The issue waits on the reporter until a
/// <see cref="QuestionAnswered"/> for the same question comes back.
/// </summary>
/// <param name="TenantId">The tenant the issue belongs to.</param>
/// <param name="QuestionId">The question, minted by the desk.</param>
/// <param name="Question">What support asks.</param>
/// <param name="Author">Who asked, as the reporter is meant to see it.</param>
[EventType]
[Public]
public record QuestionAsked(TenantId TenantId, QuestionId QuestionId, QuestionText Question, SupportAuthor Author);
