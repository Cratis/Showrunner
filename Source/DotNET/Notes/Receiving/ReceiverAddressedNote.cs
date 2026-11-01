// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Contracts.Responses;

namespace Cratis.Showrunner.Notes.Receiving;

/// <summary>
/// A version of the application that acts on a note was rolled out. It is translated from the public
/// <c>NoteAddressed</c>.
/// </summary>
/// <param name="Version">The version that acts on the note.</param>
[EventType]
public record ReceiverAddressedNote(AddressedInVersion Version);
