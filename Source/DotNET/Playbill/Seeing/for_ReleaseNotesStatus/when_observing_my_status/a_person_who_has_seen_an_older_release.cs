// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Seeing.for_ReleaseNotesStatus.when_observing_my_status;

/// <summary>
/// A person who has seen an older release is told about every release since.
/// </summary>
public class a_person_who_has_seen_an_older_release : a_status_query
{
    void Establish() => Given(People.Me, new ReleaseNotesSeen("1.0.0", People.Me));

    async Task Because() => await Ask();

    [Fact] void should_remember_how_far_they_have_seen() => Status.LastSeen.Value.ShouldEqual("1.0.0");

    [Fact] void should_count_the_newer_releases() => Status.Unread.ShouldEqual(2);

    [Fact] void should_name_them_newest_first() => Status.UnreadVersions.Select(_ => _.Value).ShouldEqual(["1.2.0", "1.1.0"]);
}
#endif
