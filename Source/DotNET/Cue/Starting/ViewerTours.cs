// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.ReadModels;

namespace Cratis.Showrunner.Cue.Starting;

/// <summary>
/// Represents an implementation of <see cref="IViewerTours"/> that reads <see cref="StartedTours"/> from the current event
/// store namespace.
/// </summary>
/// <param name="readModels">The <see cref="IReadModels"/> of the current event store namespace.</param>
public class ViewerTours(IReadModels readModels) : IViewerTours
{
    /// <inheritdoc/>
    public async Task<StartedTours> Of(ViewerSubject viewer)
    {
        var id = ViewerId.For(viewer);

        // The declared result is never null, though nobody may have started a tour yet.
        var started = await readModels.GetInstanceById<StartedTours>(id.Value);
        return started?.Tours is null ? new StartedTours(id, []) : started;
    }
}
