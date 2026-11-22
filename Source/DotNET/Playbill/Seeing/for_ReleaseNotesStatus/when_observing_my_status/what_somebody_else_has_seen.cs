// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Seeing.for_ReleaseNotesStatus.when_observing_my_status;

/// <summary>
/// What one person has seen says nothing about another.
/// </summary>
public class what_somebody_else_has_seen : a_status_query
{
    void Establish() => Given(People.SomebodyElse, new ReleaseNotesSeen("1.2.0", People.SomebodyElse));

    async Task Because() => await Ask();

    [Fact] void should_not_count_what_they_saw_as_mine() => Status.LastSeen.IsSet.ShouldBeFalse();
}
#endif
