// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.ReadModels;

namespace Cratis.Showrunner.Playbill.Seeing;

/// <summary>
/// Represents an implementation of <see cref="ISeenReleases"/> that reads <see cref="SeenReleaseNotes"/> from the current
/// event store namespace.
/// </summary>
/// <param name="readModels">The <see cref="IReadModels"/> of the current event store namespace.</param>
public class SeenReleases(IReadModels readModels) : ISeenReleases
{
    /// <inheritdoc/>
    public async Task<SeenReleaseNotes> Of(ViewerSubject viewer)
    {
        var id = ViewerId.For(viewer);

        // The declared result is never null, though nobody may have seen a release yet.
        var seen = await readModels.GetInstanceById<SeenReleaseNotes>(id.Value);
        return seen?.LastSeen is null ? new SeenReleaseNotes(id, ReleaseVersion.None) : seen;
    }
}
