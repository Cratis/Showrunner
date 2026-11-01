// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Receiving;

/// <summary>
/// The receiving side recorded a note. It is translated from the public <c>NoteAcknowledged</c>.
/// </summary>
[EventType]
public record ReceiverAcknowledgedNote;
