// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Seeing.for_ReleaseNotesStatus.when_observing_my_status;

/// <summary>
/// A person who has seen no release notes is told about the current release.
/// </summary>
public class a_person_who_has_seen_nothing : a_status_query
{
    async Task Because() => await Ask();

    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();

    [Fact] void should_name_the_running_version() => Status.Current.Value.ShouldEqual("1.2.0");

    [Fact] void should_have_seen_nothing() => Status.LastSeen.IsSet.ShouldBeFalse();

    [Fact] void should_have_one_unread_release() => Status.Unread.ShouldEqual(1);

    [Fact] void should_name_it() => Status.UnreadVersions.Select(_ => _.Value).ShouldContainOnly("1.2.0");
}
#endif
