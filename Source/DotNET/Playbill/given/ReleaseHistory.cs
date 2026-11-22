// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Playbill.given;

/// <summary>
/// Gives release histories for the specifications.
/// </summary>
public static class ReleaseHistory
{
    /// <summary>
    /// Gets a history of three releases: 1.0.0, 1.1.0 and 1.2.0, running 1.2.0.
    /// </summary>
    /// <returns>The history.</returns>
    public static ReleaseNotes ThreeReleases() => Of(new FakeReleaseNoteFiles().Release("1.0.0").Release("1.1.0").Release("1.2.0"), "1.2.0");

    /// <summary>
    /// Gets the history in some files.
    /// </summary>
    /// <param name="files">The files.</param>
    /// <param name="current">The version the application is running.</param>
    /// <returns>The history.</returns>
    public static ReleaseNotes Of(IReleaseNoteFiles files, string? current = null) =>
        new(files, Options.Create(new PlaybillOptions { CurrentVersion = current }));
}
#endif
