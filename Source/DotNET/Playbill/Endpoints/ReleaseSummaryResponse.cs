// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Playbill.Endpoints;

/// <summary>
/// A release in the list of releases.
/// </summary>
/// <param name="Version">The version of the release.</param>
/// <param name="ReleasedAt">When the release was made; <see langword="null"/> when it was not recorded.</param>
/// <param name="Pull">The pull request the release was made from; <see langword="null"/> when it was not recorded.</param>
/// <param name="Summary">One line that says what the release is about.</param>
/// <param name="Sections">The headings of the note, such as <c>Added</c> and <c>Fixed</c>.</param>
/// <param name="Assets">The files the note refers to.</param>
public record ReleaseSummaryResponse(
    string Version,
    DateTimeOffset? ReleasedAt,
    string? Pull,
    string Summary,
    IEnumerable<string> Sections,
    IEnumerable<string> Assets)
{
    /// <summary>
    /// Describes a release for the list.
    /// </summary>
    /// <param name="note">The note.</param>
    /// <returns>The response.</returns>
    public static ReleaseSummaryResponse From(ReleaseNote note) =>
        new(note.Version.Value, note.ReleasedAt, note.Pull, note.Summary, [.. note.Sections.Select(section => section.Name)], note.Assets);
}
