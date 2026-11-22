// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Playbill.Releases.for_ReleaseNoteParser.when_splitting;

/// <summary>
/// A note splits at its second-level headings; what comes before the first is the lead.
/// </summary>
public class a_note_with_sections : Specification
{
    const string Markdown = "Lead paragraph.\n\n## Added\n\n- One\n- Two\n\n### Detail\n\nMore.\n\n## Fixed\n\n- Three\n";

    string _lead;
    IReadOnlyList<ReleaseSection> _sections;

    void Because() => (_lead, _sections) = ReleaseNoteParser.Split(Markdown);

    [Fact] void should_take_what_comes_before_the_first_heading_as_the_lead() => _lead.ShouldEqual("Lead paragraph.");

    [Fact] void should_find_every_section() => _sections.Select(_ => _.Name).ShouldEqual(["Added", "Fixed"]);

    [Fact] void should_keep_what_is_under_a_heading() => _sections[0].Markdown.ShouldEqual("- One\n- Two\n\n### Detail\n\nMore.");

    [Fact] void should_keep_the_last_section() => _sections[1].Markdown.ShouldEqual("- Three");
}
#endif
