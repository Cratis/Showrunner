// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Giving;

/// <summary>
/// Published by an application that runs staged - a Play, a Preview or a sandbox the builder started to try the model - after
/// it published a note, under the note's id. It says the note was given in a staged session and not in the published
/// application. An application that is published never sends it, so a note without it was given on the published application,
/// and a receiving side that does not know it keeps working: the note itself is unchanged.
/// </summary>
/// <param name="TenantId">The tenant the note belongs to.</param>
[EventType]
[Public]
public record NoteGivenOnStagedApplication(TenantId TenantId);
