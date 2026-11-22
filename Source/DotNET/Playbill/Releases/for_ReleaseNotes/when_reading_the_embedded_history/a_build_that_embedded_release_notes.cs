// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Reflection;
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Releases.for_ReleaseNotes.when_reading_the_embedded_history;

/// <summary>
/// The Playbill build targets embed a ReleaseNotes folder, and the history is read back from the assembly: this
/// assembly is built with the targets over the fixture in given/ReleaseNotes, exactly as an application would be.
/// </summary>
public class a_build_that_embedded_release_notes : Specification
{
    EmbeddedReleaseNoteFiles _files;
    ReleaseNotes _notes;

    void Because()
    {
        _files = new EmbeddedReleaseNoteFiles([typeof(ReleaseNotes).Assembly]);
        _notes = ReleaseHistory.Of(_files);
    }

    [Fact] void should_embed_the_files_of_the_folder() => _files.Paths.ShouldContain("1.1.0/1.1.0.md", "1.1.0/1.1.0.json", "1.1.0/hero.en.png");

    [Fact] void should_not_embed_the_manifest_or_the_readme() => _files.Paths.Intersect(["versions.txt", "README.md"]).ShouldBeEmpty();

    [Fact] void should_list_the_releases_newest_first() => _notes.All.Select(_ => _.Version.Value).ShouldEqual(["1.3.0-beta.1", "1.2.0", "1.1.0", "1.0.0"]);

    [Fact] void should_read_the_record_beside_a_note() => _notes.Find("1.1.0")!.ReleasedAt.ShouldEqual(new DateTimeOffset(2026, 3, 2, 10, 0, 0, TimeSpan.Zero));

    [Fact] void should_split_a_note_into_its_sections() => _notes.Find("1.1.0")!.Sections.Select(_ => _.Name).ShouldEqual(["Changed", "Fixed"]);

    [Fact] void should_summarize_a_note_by_its_lead() => _notes.Find("1.1.0")!.Summary.ShouldEqual("Faster startup and a clearer dashboard.");

    [Fact] void should_list_the_files_a_note_refers_to() => _notes.Find("1.1.0")!.Assets.ShouldContainOnly("hero.en.png");

    [Fact] void should_serve_an_image_whose_name_looks_like_a_culture() => _notes.FindAsset("1.1.0", "hero.en.png")!.ContentType.ShouldEqual("image/png");

    [Fact] void should_not_turn_that_image_into_a_satellite_assembly() => typeof(ReleaseNotes).Assembly.GetManifestResourceInfo("Playbill/1.1.0/hero.en.png")!.ResourceLocation.ShouldEqual(ResourceLocation.Embedded | ResourceLocation.ContainedInManifestFile);

    [Fact] void should_give_the_same_fingerprint_for_the_same_build() => _files.Fingerprint.ShouldEqual(new EmbeddedReleaseNoteFiles([typeof(ReleaseNotes).Assembly]).Fingerprint);
}
#endif
