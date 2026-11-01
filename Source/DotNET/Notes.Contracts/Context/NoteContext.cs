// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Context;

/// <summary>
/// What the user was looking at when they gave a note.
/// </summary>
/// <param name="Location">Where in the application the user was: the module, feature and slice Arc resolves for what is on screen.</param>
/// <param name="ActiveSlices">Every slice active on the screen; may be empty.</param>
/// <param name="Screen">The screen.</param>
/// <param name="Route">The route.</param>
/// <param name="Dialog">The open dialog, <see cref="NoteDialog.NoDialog"/> when none was open.</param>
/// <param name="ModelReferences">Identifiers of model elements, supplied by the host and keyed by what they identify; may be empty.</param>
/// <param name="CorrelationId">The correlation id of the command or query that ran last in the user's view, when there is one.</param>
public record NoteContext(
    SliceLocation Location,
    IEnumerable<SliceLocation> ActiveSlices,
    NoteScreen Screen,
    Route Route,
    NoteDialog Dialog,
    IDictionary<string, string> ModelReferences,
    CorrelationId CorrelationId);
