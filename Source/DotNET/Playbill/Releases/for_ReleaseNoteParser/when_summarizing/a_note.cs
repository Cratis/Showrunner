// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Playbill.Releases.for_ReleaseNoteParser.when_summarizing;

/// <summary>
/// The summary is the lead when there is one, and otherwise the first thing a section lists, without markdown or issue references.
/// </summary>
public class a_note : Specification
{
    static string Summary(string markdown)
    {
        var (lead, sections) = ReleaseNoteParser.Split(markdown);
        return ReleaseNoteParser.Summarize(lead, sections);
    }

    [Fact] void should_use_the_lead_paragraph() => Summary("Faster **startup**.\n\nMore.\n\n## Fixed\n\n- A thing.").ShouldEqual("Faster startup.");

    [Fact] void should_skip_a_lead_that_is_only_an_image() => Summary("![hero](hero.png)\n\n## Fixed\n\n- A thing.").ShouldEqual("A thing.");

    [Fact] void should_use_the_first_bullet_without_the_issue_reference() => Summary("## Added\n\n- A [new page](https://example.com) for `reports`. (#12)").ShouldEqual("A new page for reports.");

    [Fact] void should_drop_a_part_of_reference() => Summary("## Fixed\n\n- Partly fixed. (part of Cratis/Showrunner#12)").ShouldEqual("Partly fixed.");

    [Fact] void should_be_empty_when_the_note_says_nothing() => Summary("").ShouldEqual(string.Empty);

    [Fact] void should_cut_a_long_line() => Summary($"## Added\n\n- {new string('a', 400)}").Length.ShouldEqual(200);
}
#endif
