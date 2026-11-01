// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Responses;

/// <summary>
/// Published by the receiving side when it has recorded a note, so the giver can see that it arrived.
/// </summary>
/// <param name="TenantId">The tenant the note belongs to.</param>
[EventType]
[Public]
public record NoteAcknowledged(TenantId TenantId);
