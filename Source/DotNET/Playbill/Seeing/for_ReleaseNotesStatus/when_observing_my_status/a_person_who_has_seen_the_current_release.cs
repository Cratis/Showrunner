// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Seeing.for_ReleaseNotesStatus.when_observing_my_status;

/// <summary>
/// A person who is up to date has nothing unread.
/// </summary>
public class a_person_who_has_seen_the_current_release : a_status_query
{
    void Establish() => Given(People.Me, new ReleaseNotesSeen("1.2.0", People.Me));

    async Task Because() => await Ask();

    [Fact] void should_have_nothing_unread() => Status.Unread.ShouldEqual(0);

    [Fact] void should_name_no_versions() => Status.UnreadVersions.ShouldBeEmpty();
}
#endif
