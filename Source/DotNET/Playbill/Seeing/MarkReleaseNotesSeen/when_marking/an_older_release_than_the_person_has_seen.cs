// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Seeing.MarkReleaseNotesSeen.when_marking;

/// <summary>
/// Seeing an older release again never moves the person back: nothing is appended.
/// </summary>
public class an_older_release_than_the_person_has_seen : a_marking_viewer
{
    void Establish() => Given(new ReleaseNotesSeen("1.2.0", People.Me));

    async Task Because() => _result = await _scenario.Execute(new MarkReleaseNotesSeen("1.0.0"));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
