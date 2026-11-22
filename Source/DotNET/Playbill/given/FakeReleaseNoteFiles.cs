// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text;

namespace Cratis.Showrunner.Playbill.given;

/// <summary>
/// An <see cref="IReleaseNoteFiles"/> that holds its files in memory.
/// </summary>
public class FakeReleaseNoteFiles : IReleaseNoteFiles
{
    readonly Dictionary<string, byte[]> _files = [];

    /// <inheritdoc/>
    public IReadOnlyList<string> Paths => [.. _files.Keys.Order(StringComparer.Ordinal)];

    /// <inheritdoc/>
    public string Fingerprint => "fake";

    /// <summary>
    /// Adds a text file.
    /// </summary>
    /// <param name="path">The path inside the release-notes folder.</param>
    /// <param name="text">The content.</param>
    /// <returns>The files, for continuation.</returns>
    public FakeReleaseNoteFiles With(string path, string text)
    {
        _files[path] = Encoding.UTF8.GetBytes(text);
        return this;
    }

    /// <summary>
    /// Adds a release with a note.
    /// </summary>
    /// <param name="version">The version.</param>
    /// <param name="markdown">The note.</param>
    /// <returns>The files, for continuation.</returns>
    public FakeReleaseNoteFiles Release(string version, string markdown = "## Fixed\n\n- Something. (#1)\n") => With($"{version}/{version}.md", markdown);

    /// <inheritdoc/>
    public Stream Open(string path) => new MemoryStream(_files[path]);
}
#endif
