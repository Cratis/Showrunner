// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Transport;

/// <summary>
/// What the receiving side did with a note, as the collector hands it to the application.
/// </summary>
/// <param name="Cursor">The position of the answer; ask for the answers <c>after</c> the highest cursor seen. It is above zero.</param>
/// <param name="NoteId">The note.</param>
/// <param name="Kind">What was done.</param>
/// <param name="Occurred">When the receiving side did it.</param>
/// <param name="Version">The version that acts on the note; present for <see cref="NoteResponseKind.Addressed"/>.</param>
/// <param name="Reason">Why the note will not be acted on; present for <see cref="NoteResponseKind.Declined"/>.</param>
public record NoteResponse(
    ulong Cursor,
    Guid NoteId,
    NoteResponseKind Kind,
    DateTimeOffset Occurred,
    string? Version = null,
    string? Reason = null);
