// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Seeing.for_SeenReleaseNotes.when_reducing;

/// <summary>
/// How far a person has seen only moves forward, whatever order the events are replayed in.
/// </summary>
public class seeing_forward_and_back : a_read_model<SeenReleaseNotes>
{
    SeenReleaseNotes _seen;

    async Task Because()
    {
        await Happened(
            new ReleaseNotesSeen("1.0.0", People.Me),
            new ReleaseNotesSeen("1.2.0", People.Me),
            new ReleaseNotesSeen("1.1.0", People.Me));
        _seen = _scenario.Instance;
    }

    [Fact] void should_keep_the_newest_release_seen() => _seen.LastSeen.Value.ShouldEqual("1.2.0");
}
#endif
