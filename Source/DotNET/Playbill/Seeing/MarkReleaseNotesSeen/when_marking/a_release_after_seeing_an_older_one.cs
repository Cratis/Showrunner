// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Seeing.MarkReleaseNotesSeen.when_marking;

/// <summary>
/// Marking a newer release than the person has seen moves them forward.
/// </summary>
public class a_release_after_seeing_an_older_one : a_marking_viewer
{
    void Establish() => Given(new ReleaseNotesSeen("1.0.0", People.Me));

    async Task Because() => _result = await _scenario.Execute(new MarkReleaseNotesSeen("1.2.0"));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_that_the_person_has_seen_it() =>
        await _scenario.ShouldHaveAppendedEvent<MarkReleaseNotesSeen, ReleaseNotesSeen>(
            ViewerId.For(People.Me),
            e => e.Version.Value == "1.2.0");
}
#endif
