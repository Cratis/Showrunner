// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Responses;

/// <summary>
/// Published by the receiving side when it decided not to act on a note.
/// </summary>
/// <param name="TenantId">The tenant the note belongs to.</param>
/// <param name="Reason">Why it will not act on the note.</param>
[EventType]
[Public]
public record NoteDeclined(TenantId TenantId, DeclineReason Reason);
