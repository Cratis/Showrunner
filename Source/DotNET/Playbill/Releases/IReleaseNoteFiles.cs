// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Playbill.Releases;

/// <summary>
/// Defines where the files of the release history are kept: one folder for each version, holding the note, an optional
/// record of how the release was made and the files the note refers to.
/// </summary>
public interface IReleaseNoteFiles
{
    /// <summary>
    /// Gets the path of every file, relative to the release-notes folder and with forward slashes, such as
    /// <c>2.3.0/2.3.0.md</c>.
    /// </summary>
    IReadOnlyList<string> Paths { get; }

    /// <summary>
    /// Gets a value that changes whenever the content of the files does, for validating cached responses.
    /// </summary>
    string Fingerprint { get; }

    /// <summary>
    /// Opens a file for reading.
    /// </summary>
    /// <param name="path">A path from <see cref="Paths"/>.</param>
    /// <returns>The content of the file.</returns>
    Stream Open(string path);
}
