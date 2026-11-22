// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Playbill.Endpoints;

/// <summary>
/// One release, with its note.
/// </summary>
/// <param name="Version">The version of the release.</param>
/// <param name="ReleasedAt">When the release was made; <see langword="null"/> when it was not recorded.</param>
/// <param name="Pull">The pull request the release was made from; <see langword="null"/> when it was not recorded.</param>
/// <param name="Summary">One line that says what the release is about.</param>
/// <param name="Lead">What is written before the first section, as markdown; empty when the note starts with a section.</param>
/// <param name="Sections">The sections of the note, in the order they are written.</param>
/// <param name="Assets">The files the note refers to.</param>
/// <param name="Markdown">The note, as it was captured. Relative links and images point into the release's own folder.</param>
public record ReleaseResponse(
    string Version,
    DateTimeOffset? ReleasedAt,
    string? Pull,
    string Summary,
    string Lead,
    IEnumerable<ReleaseSectionResponse> Sections,
    IEnumerable<string> Assets,
    string Markdown)
{
    /// <summary>
    /// Describes a release with its note.
    /// </summary>
    /// <param name="note">The note.</param>
    /// <returns>The response.</returns>
    public static ReleaseResponse From(ReleaseNote note) =>
        new(
            note.Version.Value,
            note.ReleasedAt,
            note.Pull,
            note.Summary,
            note.Lead,
            [.. note.Sections.Select(section => new ReleaseSectionResponse(section.Name, section.Markdown))],
            note.Assets,
            note.Markdown);
}
