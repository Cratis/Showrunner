// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Playbill.Seeing.for_UnreadReleases.when_finding_what_is_unread;

/// <summary>
/// What a person has not seen is everything newer than the newest release they have seen; a person who has seen none is
/// told about the current release, not the whole history.
/// </summary>
public class a_person_who_has_seen_some : Specification
{
    static readonly ReleaseVersion[] Releases = ["2.0.0", "1.2.0", "1.1.0", "1.0.0"];

    static IEnumerable<string> Unread(ReleaseVersion lastSeen) => UnreadReleases.Since(Releases, lastSeen).Select(_ => _.Value);

    [Fact] void should_find_every_newer_release_newest_first() => Unread("1.0.0").ShouldEqual(["2.0.0", "1.2.0", "1.1.0"]);

    [Fact] void should_find_none_when_they_have_seen_the_newest() => Unread("2.0.0").ShouldBeEmpty();

    [Fact] void should_find_only_the_current_release_when_they_have_seen_none() => Unread(ReleaseVersion.None).ShouldContainOnly("2.0.0");

    [Fact] void should_find_none_when_there_are_no_releases() => UnreadReleases.Since([], ReleaseVersion.None).ShouldBeEmpty();
}
#endif
