// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Playbill.Releases;

/// <summary>
/// What is known about a release: what changed in it and when and how it was made.
/// </summary>
/// <param name="Version">The version of the release.</param>
/// <param name="ReleasedAt">When the release was made; <see langword="null"/> when the capture did not record it.</param>
/// <param name="Pull">The pull request the release was made from; <see langword="null"/> when the capture did not record it.</param>
/// <param name="Summary">One line that says what the release is about.</param>
/// <param name="Lead">What is written before the first section, as markdown; empty when the note starts with a section.</param>
/// <param name="Sections">The sections of the note, in the order they are written.</param>
/// <param name="Assets">The files the note refers to, such as images, by their name inside the release's folder.</param>
/// <param name="Markdown">The note, as it was captured.</param>
public record ReleaseNote(
    ReleaseVersion Version,
    DateTimeOffset? ReleasedAt,
    string? Pull,
    string Summary,
    string Lead,
    IReadOnlyList<ReleaseSection> Sections,
    IReadOnlyList<string> Assets,
    string Markdown);
