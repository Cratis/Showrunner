// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Playbill.Releases;

/// <summary>
/// Represents an implementation of <see cref="IReleaseNotes"/> that reads the release history from
/// <see cref="IReleaseNoteFiles"/> once, the first time it is asked.
/// </summary>
/// <param name="files">The <see cref="IReleaseNoteFiles"/> that hold the history.</param>
/// <param name="options">The <see cref="PlaybillOptions"/>.</param>
/// <remarks>
/// A release is a folder named after its version that holds <c>&lt;version&gt;.md</c>. <c>&lt;version&gt;.json</c> may record when
/// the release was made and from which pull request, and every other file is an asset the note can refer to. Folders that
/// are not versions, and versions without a note, are not releases. The history never changes while the application runs,
/// because it was embedded when the application was built.
/// </remarks>
public class ReleaseNotes(IReleaseNoteFiles files, IOptions<PlaybillOptions> options) : IReleaseNotes
{
    static readonly FileExtensionContentTypeProvider _contentTypes = new();

    readonly Lazy<Catalog> _catalog = new(() => Read(files));

    /// <inheritdoc/>
    public IReadOnlyList<ReleaseNote> All => _catalog.Value.Notes;

    /// <inheritdoc/>
    public ReleaseVersion Current { get; } = options.Value.ResolveCurrentVersion();

    /// <inheritdoc/>
    public string Fingerprint => files.Fingerprint;

    /// <inheritdoc/>
    public ReleaseNote? Find(ReleaseVersion version) => _catalog.Value.ByVersion.GetValueOrDefault(version.Value);

    /// <inheritdoc/>
    public ReleaseAsset? FindAsset(ReleaseVersion version, string name)
    {
        var catalog = _catalog.Value;
        return catalog.Assets.GetValueOrDefault((version.Value, name));
    }

    static Catalog Read(IReleaseNoteFiles files)
    {
        var notes = new List<ReleaseNote>();
        var assets = new Dictionary<(string Version, string Name), ReleaseAsset>();
        foreach (var folder in files.Paths.Where(path => path.Contains('/')).GroupBy(path => path[..path.IndexOf('/')]))
        {
            var version = new ReleaseVersion(folder.Key);
            if (!version.IsValid)
            {
                continue;
            }

            var notePath = $"{folder.Key}/{folder.Key}.md";
            var metadataPath = $"{folder.Key}/{folder.Key}.json";
            if (!folder.Contains(notePath))
            {
                continue;
            }

            var markdown = ReadText(files, notePath).Trim();
            var (lead, sections) = ReleaseNoteParser.Split(markdown);
            var (releasedAt, pull) = ReleaseNoteParser.ReadMetadata(folder.Contains(metadataPath) ? ReadText(files, metadataPath) : null);
            var names = new List<string>();
            foreach (var path in folder.Where(path => path != notePath && path != metadataPath).Order(StringComparer.Ordinal))
            {
                var name = path[(folder.Key.Length + 1)..];
                var captured = path;
                names.Add(name);
                assets[(folder.Key, name)] = new(name, ContentTypeOf(name), () => files.Open(captured));
            }

            notes.Add(new(version, releasedAt, pull, ReleaseNoteParser.Summarize(lead, sections), lead, sections, names, markdown));
        }

        notes.Sort((first, second) => ReleaseVersion.Compare(second.Version, first.Version) is var order and not 0
            ? order
            : string.CompareOrdinal(second.Version.Value, first.Version.Value));
        return new([.. notes], notes.ToDictionary(note => note.Version.Value, StringComparer.Ordinal), assets);
    }

    static string ReadText(IReleaseNoteFiles files, string path)
    {
        using var reader = new StreamReader(files.Open(path), Encoding.UTF8);
        return reader.ReadToEnd();
    }

    static string ContentTypeOf(string name) =>
        _contentTypes.TryGetContentType(name, out var contentType) ? contentType : "application/octet-stream";

    sealed record Catalog(
        IReadOnlyList<ReleaseNote> Notes,
        Dictionary<string, ReleaseNote> ByVersion,
        Dictionary<(string Version, string Name), ReleaseAsset> Assets);
}
