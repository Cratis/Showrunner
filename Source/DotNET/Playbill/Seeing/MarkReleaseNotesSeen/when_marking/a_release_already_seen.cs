// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Seeing.MarkReleaseNotesSeen.when_marking;

/// <summary>
/// Marking a release the person has already seen appends nothing.
/// </summary>
public class a_release_already_seen : a_marking_viewer
{
    void Establish() => Given(new ReleaseNotesSeen("1.1.0", People.Me));

    async Task Because() => _result = await _scenario.Execute(new MarkReleaseNotesSeen("1.1.0"));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
