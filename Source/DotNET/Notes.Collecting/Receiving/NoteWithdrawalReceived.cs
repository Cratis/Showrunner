// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Collecting.Receiving;

/// <summary>
/// The giver withdrew a note. It is translated from the public <c>NoteWithdrawn</c>; nothing is acted on a withdrawn note
/// afterwards.
/// </summary>
[EventType]
public record NoteWithdrawalReceived;
