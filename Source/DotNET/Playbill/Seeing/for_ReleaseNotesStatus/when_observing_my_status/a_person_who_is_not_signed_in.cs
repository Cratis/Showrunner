// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Seeing.for_ReleaseNotesStatus.when_observing_my_status;

/// <summary>
/// Someone who is not signed in is refused and is given no status.
/// </summary>
public class a_person_who_is_not_signed_in : a_status_query
{
    protected override ClaimsPrincipal Principal => People.Anonymous;

    void Establish() => Given(People.Me, new ReleaseNotesSeen("1.0.0", People.Me));

    async Task Because() => await Ask();

    [Fact] void should_be_refused() => _result.IsAuthorized.ShouldBeFalse();

    [Fact] void should_give_no_status() => (_result.Data is null || !((IEnumerable<ReleaseNotesStatus>)_result.Data).Any()).ShouldBeTrue();
}
#endif
