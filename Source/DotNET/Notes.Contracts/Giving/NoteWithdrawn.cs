// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Giving;

/// <summary>
/// Published by the application when the giver withdraws a note. The receiving side stops acting on it.
/// </summary>
/// <param name="TenantId">The tenant the note belongs to.</param>
[EventType]
[Public]
public record NoteWithdrawn(TenantId TenantId);
