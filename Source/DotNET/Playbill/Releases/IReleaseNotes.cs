// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Playbill.Releases;

/// <summary>
/// Defines the release history the application was built with.
/// </summary>
public interface IReleaseNotes
{
    /// <summary>
    /// Gets every release that has a note, the newest first.
    /// </summary>
    IReadOnlyList<ReleaseNote> All { get; }

    /// <summary>
    /// Gets the version the application is running; <see cref="ReleaseVersion.None"/> when it is not known.
    /// </summary>
    ReleaseVersion Current { get; }

    /// <summary>
    /// Gets a value that changes whenever the release history does, for validating cached responses.
    /// </summary>
    string Fingerprint { get; }

    /// <summary>
    /// Finds the note of a release.
    /// </summary>
    /// <param name="version">The version of the release.</param>
    /// <returns>The note, or <see langword="null"/> when the release has none.</returns>
    ReleaseNote? Find(ReleaseVersion version);

    /// <summary>
    /// Finds a file a note refers to, such as an image.
    /// </summary>
    /// <param name="version">The version of the release.</param>
    /// <param name="name">The name of the file inside the release's folder.</param>
    /// <returns>The file, or <see langword="null"/> when the release has no such file.</returns>
    ReleaseAsset? FindAsset(ReleaseVersion version, string name);
}
