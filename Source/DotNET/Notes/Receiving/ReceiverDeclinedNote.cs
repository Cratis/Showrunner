// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Contracts.Responses;

namespace Cratis.Showrunner.Notes.Receiving;

/// <summary>
/// The receiving side decided not to act on a note. It is translated from the public <c>NoteDeclined</c>.
/// </summary>
/// <param name="Reason">Why it will not act on the note.</param>
[EventType]
public record ReceiverDeclinedNote(DeclineReason Reason);
