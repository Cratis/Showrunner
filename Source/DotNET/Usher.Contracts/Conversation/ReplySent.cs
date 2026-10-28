// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Conversation;

/// <summary>
/// Published by the desk when support writes to the reporter without asking anything.
/// </summary>
/// <param name="TenantId">The tenant the issue belongs to.</param>
/// <param name="Reply">What support wrote.</param>
/// <param name="Author">Who wrote it, as the reporter is meant to see it.</param>
[EventType]
[Public]
public record ReplySent(TenantId TenantId, ReplyText Reply, SupportAuthor Author);
