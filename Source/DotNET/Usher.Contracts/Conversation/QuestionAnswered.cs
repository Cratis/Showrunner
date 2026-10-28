// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Conversation;

/// <summary>
/// Published by the application when the reporter answers a question support asked with <see cref="QuestionAsked"/>.
/// </summary>
/// <param name="TenantId">The tenant the issue belongs to.</param>
/// <param name="QuestionId">The question being answered.</param>
/// <param name="Answer">The reporter's answer.</param>
[EventType]
[Public]
public record QuestionAnswered(TenantId TenantId, QuestionId QuestionId, Answer Answer);
