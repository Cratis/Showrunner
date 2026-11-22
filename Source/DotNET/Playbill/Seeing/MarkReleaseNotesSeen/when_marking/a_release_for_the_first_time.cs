// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Seeing.MarkReleaseNotesSeen.when_marking;

/// <summary>
/// Marking a release as seen appends that the person has seen the notes up to it.
/// </summary>
public class a_release_for_the_first_time : a_marking_viewer
{
    async Task Because() => _result = await _scenario.Execute(new MarkReleaseNotesSeen("1.1.0"));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_that_the_person_has_seen_it() =>
        await _scenario.ShouldHaveAppendedEvent<MarkReleaseNotesSeen, ReleaseNotesSeen>(
            ViewerId.For(People.Me),
            e => e.Version.Value == "1.1.0" && e.Viewer == People.Me);
}
#endif
