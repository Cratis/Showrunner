// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Playbill.Releases.for_ReleaseNoteParser.when_splitting;

/// <summary>
/// A heading inside fenced code is text, not a section.
/// </summary>
public class a_heading_inside_fenced_code : Specification
{
    const string Markdown = "## Added\n\n- Docs now show:\n\n```md\n## Not a section\n```\n";

    IReadOnlyList<ReleaseSection> _sections;

    void Because() => (_, _sections) = ReleaseNoteParser.Split(Markdown);

    [Fact] void should_find_one_section() => _sections.Select(_ => _.Name).ShouldContainOnly("Added");

    [Fact] void should_keep_the_fenced_code_in_it() => _sections[0].Markdown.ShouldContain("## Not a section");
}
#endif
