// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Seeing.for_SeenReleaseNotes.when_reducing;

/// <summary>
/// The first release a person sees is how far they have seen.
/// </summary>
public class a_person_seeing_a_release : a_read_model<SeenReleaseNotes>
{
    SeenReleaseNotes _seen;

    async Task Because()
    {
        await Happened(new ReleaseNotesSeen("1.1.0", People.Me));
        _seen = _scenario.Instance;
    }

    [Fact] void should_have_seen_that_release() => _seen.LastSeen.Value.ShouldEqual("1.1.0");

    [Fact] void should_belong_to_the_person() => _seen.Id.ShouldEqual(ViewerId.For(People.Me));

    [Fact] void should_have_seen_older_releases_too() => _seen.HasSeen("1.0.0").ShouldBeTrue();

    [Fact] void should_not_have_seen_newer_releases() => _seen.HasSeen("1.2.0").ShouldBeFalse();
}
#endif
