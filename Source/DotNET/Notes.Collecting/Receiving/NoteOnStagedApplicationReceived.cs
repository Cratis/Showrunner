// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Collecting.Receiving;

/// <summary>
/// The note was given in a staged session - a Play, a Preview or a sandbox - and not on the published application. It is
/// translated from the public <c>NoteGivenOnStagedApplication</c>, recorded under the note's id after the fact that opened the
/// note. A note without it was given on the published application.
/// </summary>
[EventType]
public record NoteOnStagedApplicationReceived;
