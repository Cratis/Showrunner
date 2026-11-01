// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Responses;

/// <summary>
/// Published by the receiving side when a version of the application that acts on a note was rolled out.
/// </summary>
/// <param name="TenantId">The tenant the note belongs to.</param>
/// <param name="AddressedInVersion">The version that acts on the note.</param>
[EventType]
[Public]
public record NoteAddressed(TenantId TenantId, AddressedInVersion AddressedInVersion);
