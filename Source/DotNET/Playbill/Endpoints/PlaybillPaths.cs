// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Playbill.Endpoints;

/// <summary>
/// The paths Playbill serves the release history under. They are versioned, and anything that reads the history, such
/// as the React components or an operator's tooling, can rely on them.
/// </summary>
public static class PlaybillPaths
{
    /// <summary>
    /// The path every endpoint is under.
    /// </summary>
    public const string Group = "/.cratis/playbill/v1";

    /// <summary>
    /// The path of the list of releases, relative to <see cref="Group"/>.
    /// </summary>
    public const string Releases = "/releases";

    /// <summary>
    /// The path of one release, relative to <see cref="Group"/>.
    /// </summary>
    public const string Release = "/releases/{version}";

    /// <summary>
    /// The path of a file a release note refers to, relative to <see cref="Group"/>.
    /// </summary>
    public const string Asset = "/releases/{version}/{**asset}";

    /// <summary>
    /// The path of the running version, relative to <see cref="Group"/>.
    /// </summary>
    public const string Current = "/current";
}
