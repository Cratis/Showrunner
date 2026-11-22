// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Releases.for_ReleaseNotes.when_reading_the_history;

/// <summary>
/// Only a folder named after a version that holds that version's note is a release.
/// </summary>
public class files_that_are_not_releases : Specification
{
    ReleaseNotes _notes;

    void Because() => _notes = ReleaseHistory.Of(new FakeReleaseNoteFiles()
        .Release("1.0.0")
        .Release("latest")
        .With("1.1.0/screenshot.png", "image")
        .With("versions.txt", "1.0.0")
        .With("1.0.0/other.md", "Not the note"));

    [Fact] void should_list_only_the_release_with_a_note_and_a_version() => _notes.All.Select(_ => _.Version.Value).ShouldContainOnly("1.0.0");

    [Fact] void should_not_find_a_folder_that_is_not_a_version() => _notes.Find("latest").ShouldBeNull();

    [Fact] void should_not_find_a_version_without_a_note() => _notes.Find("1.1.0").ShouldBeNull();

    [Fact] void should_not_serve_a_file_of_a_version_without_a_note() => _notes.FindAsset("1.1.0", "screenshot.png").ShouldBeNull();

    [Fact] void should_treat_another_markdown_file_as_an_asset() => _notes.Find("1.0.0")!.Assets.ShouldContainOnly("other.md");
}
#endif
